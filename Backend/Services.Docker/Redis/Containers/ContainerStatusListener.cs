using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Database;
using Services.Docker.Services;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Requests.Containers;
using Services.Docker.Shared.Messages.Responses.Containers;
using Services.Docker.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Docker.Redis.Containers;

public class ContainerStatusListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerContainerService _dockerContainerService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ContainerStatusListener);
    
    public ContainerStatusListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerContainerService dockerContainerService)
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

        await subscriber.SubscribeAsync(DockerRedisChannels.ContainerStatusChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ContainerStatusChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ContainerStatusRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        var container = await _dockerContainerService.ContainerOneAsync(request.ContainerId);
        if (container == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse, 
                ErrorResponseGenerator.GetContainerNotFoundResponse(request.ConnectionId, ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        var dbContainer = await _dockerDbContext.DockerContainers.FirstOrDefaultAsync(e => e.ContainerId == request.ContainerId);
        if (dbContainer == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse, 
                ErrorResponseGenerator.GetContainerNotFoundResponse(request.ConnectionId, ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        dbContainer.Status = DockerStatusToEnum.Convert(container.Status);

        await publisher.PublishAsync(
            DockerRedisChannels.ContainerStatusChannelResponse,
            JsonSerializer.Serialize(new ContainerResponse
            {
                Id = dbContainer.Id,
                ConnectionId = request.ConnectionId,

                UserId = dbContainer.UserId,
                ContainerId = dbContainer.ContainerId,

                Status = dbContainer.Status,
                Logs = dbContainer.Logs,
                
                ProgramCode = dbContainer.ProgramCode,
                ProgramCodeFolder = dbContainer.ProgramCodeFolder,

                // TODO: Нужен фоновый процесс который будет отслеживать эти параметры (ну и логи со статусом тоже)
                UsageMemory = dbContainer.UsageMemory,
                UsageCpu = dbContainer.UsageCpu,
                UsageStorage = dbContainer.UsageStorage,
            })
        );
    }
}