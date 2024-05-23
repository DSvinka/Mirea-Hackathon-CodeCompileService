using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Database;
using Services.Docker.Services;
using Services.Docker.Shared;
using Services.Docker.Shared.Enums;
using Services.Docker.Shared.Messages.Responses.Containers;
using Services.Docker.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Docker.Workers;

public class ContainerMonitoringWorker: BackgroundService
{
    private readonly ILogger<ContainerMonitoringWorker> _logger;
    private readonly DockerContainerService _dockerContainerService;
    private readonly DockerDbContext _dockerDbContext;

    private readonly ISubscriber _publisher;
    
    public ContainerMonitoringWorker(ILogger<ContainerMonitoringWorker> logger, DockerContainerService dockerContainerService, DockerDbContext dockerDbContext, RedisService redisService)
    {
        _logger = logger;
        _dockerContainerService = dockerContainerService;
        _dockerDbContext = dockerDbContext;
        _publisher = redisService.Connection.GetSubscriber();
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await CheckContainers(cancellationToken);
            await CheckUnusedCodeFolders(cancellationToken);
            
            await Task.Delay(5 * 1000, cancellationToken);  // 5 секунд
        }
    }

    public async Task CheckContainers(CancellationToken cancellationToken)
    {
        var containers = await _dockerContainerService.ContainerListAsync(cancellationToken);
        if (containers != null && containers.Count != 0)
        {
            foreach (var container in containers)
            {
                var dbContainer = await _dockerDbContext.DockerContainers.FirstOrDefaultAsync(e => e.ContainerId == container.ID, cancellationToken: cancellationToken);
                if (dbContainer == null)
                {
                    await _dockerContainerService.TryContainerStopAsync(container.ID, cancellationToken);
                    await _dockerContainerService.TryContainerDeleteAsync(container.ID, cancellationToken);

                    continue;
                };
                    
                dbContainer.Status = DockerStatusToEnum.Convert(container.Status);
                dbContainer.Logs = await _dockerContainerService.ContainerLogsAsync(container.ID, cancellationToken);

                var stats = await _dockerContainerService.ContainerStatsAsync(container.ID, cancellationToken);
                
                dbContainer.UsageMemory = (int) stats.MemoryStats.Usage;
                dbContainer.UsageCpu = (int) stats.CPUStats.CPUUsage.TotalUsage;

                if (dbContainer.Status == EDockerStatus.Unknown || dbContainer.Status == EDockerStatus.Dead || dbContainer.Status == EDockerStatus.Exist)
                {
                    await _dockerContainerService.TryContainerStopAsync(container.ID, cancellationToken);
                    await _dockerContainerService.TryContainerDeleteAsync(container.ID, cancellationToken);

                    _dockerDbContext.DockerContainers.Remove(dbContainer);
                    await _dockerDbContext.SaveChangesAsync(cancellationToken);
                }

                await _publisher.PublishAsync(
                    DockerRedisChannels.ContainerStatusChannelResponse,
                    JsonSerializer.Serialize(new ContainerResponse
                    {
                        Id = dbContainer.Id,
                        UserId = dbContainer.UserId,
                        ContainerId = dbContainer.ContainerId,
                        
                        Status = dbContainer.Status,
                        
                        ProgramCode = dbContainer.ProgramCode,
                        ProgramCodeFolder = dbContainer.ProgramCodeFolder,
                        
                        UsageMemory = dbContainer.UsageMemory,
                        UsageCpu = dbContainer.UsageCpu,
                        UsageStorage = dbContainer.UsageStorage
                    })
                );
            }
        }
    }

    public async Task CheckUnusedCodeFolders(CancellationToken cancellationToken)
    {
        
    }
}