using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Database;
using Services.Docker.Database.Models;
using Services.Docker.Services;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Requests.Containers;
using Services.Docker.Shared.Messages.Responses.Containers;
using Services.Docker.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Docker.Redis.Containers;

public class ContainerListListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerContainerService _dockerContainerService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ContainerListListener);
    
    public ContainerListListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerContainerService dockerContainerService)
    {
        _redisService = redisService;
        _dockerDbContext = dockerDbContext;
        _dockerContainerService = dockerContainerService;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{ListenerName} is starting.");

        stoppingToken.Register(() => _logger.LogInformation($"{ListenerName} is stopping."));

        var subscriber = _redisService.Connection.GetSubscriber();

        await subscriber.SubscribeAsync(DockerRedisChannels.ContainerListChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ContainerListChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ContainerListRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        IQueryable<DockerContainerModel> dbContainersQuery = _dockerDbContext.DockerContainers.AsQueryable();

        if (request.FilterUserId != null) dbContainersQuery = dbContainersQuery.Where(e => e.UserId == request.FilterUserId);
        if (request.FilterImageId != null) dbContainersQuery = dbContainersQuery.Where(e => e.ImageId == request.FilterImageId);

        var dbContainers = await dbContainersQuery.ToListAsync();
        
        var containerResponses = new List<ContainerResponse>(dbContainers.Count);
        containerResponses.AddRange(
            dbContainers.Select(container => new ContainerResponse
            {
                Id = container.Id,
                ConnectionId = request.ConnectionId,

                UserId = container.UserId,
                ContainerId = container.ContainerId,

                Status = container.Status,
                Logs = container.Logs,
                
                ProgramCode = container.ProgramCode,
                ProgramCodeFolder = container.ProgramCodeFolder,

                // TODO: Нужен фоновый процесс который будет отслеживать эти параметры (ну и логи со статусом тоже)
                UsageMemory = container.UsageMemory,
                UsageCpu = container.UsageCpu,
                UsageStorage = container.UsageStorage
            })
        );

        await publisher.PublishAsync(
            DockerRedisChannels.ContainerListChannelResponse,
            JsonSerializer.Serialize(new ContainerListResponse
            {
                ConnectionId = request.ConnectionId,
                
                Containers = containerResponses
            })
        );
    }
}