using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Database;
using Services.Docker.Database.Models;
using Services.Docker.Services;
using Services.Docker.Shared;
using Services.Docker.Shared.Enums;
using Services.Docker.Shared.Messages.Requests.Containers;
using Services.Docker.Shared.Messages.Responses.Containers;
using Services.Docker.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Docker.Redis.Containers;

public class ContainerCreateAndRunListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerContainerService _dockerContainerService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ContainerCreateAndRunListener);
    
    public ContainerCreateAndRunListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerContainerService dockerContainerService)
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

        await subscriber.SubscribeAsync(DockerRedisChannels.ContainerCreateAndRunChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ContainerCreateAndRunChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ContainerCreateAndRunRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        var image = await _dockerDbContext.DockerImages.FirstOrDefaultAsync(e => e.Id == request.ImageId);
        if (image == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse,
                ErrorResponseGenerator.GetImageNotFoundResponse(request.ConnectionId, ListenerName),
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        var containers = await _dockerContainerService.ContainerListByUserAsync(request.UserId.ToString());
        if (containers != null && containers.Count >= image.MaxCountByUser && image.MaxCountByUser != -1)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse,
                ErrorResponseGenerator.GetToManyUserContainersResponse(request.ConnectionId, ListenerName),
                CommandFlags.FireAndForget
            );
            
            return;
        }

        var containersServer = await _dockerContainerService.ContainerListAsync();
        if (containersServer != null && containersServer.Count >= image.MaxCountByServer && image.MaxCountByServer != -1)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerErrorChannelResponse,
                ErrorResponseGenerator.GetToManyServerContainersResponse(request.ConnectionId, ListenerName),
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        var container = new DockerContainerModel
        {
            UserId = request.UserId,
            ImageId = image.Id,
            ContainerId = null,
            
            ProgramCode = request.ProgramCode,
            
            Status = EDockerStatus.Created,
        };

        (container.ContainerId, container.ProgramCodeFolder) = await _dockerContainerService.ContainerCreateAsync(image, request.UserId.ToString(), request.ProgramCode);
        
        await _dockerDbContext.DockerContainers.AddAsync(container);
        await _dockerDbContext.SaveChangesAsync();
        await _dockerDbContext.Entry(container).GetDatabaseValuesAsync();

        await publisher.PublishAsync(
            DockerRedisChannels.ContainerCreateAndRunChannelResponse,
            JsonSerializer.Serialize(new ContainerCreateAndRunResponse()
            {
                ConnectionId = request.ConnectionId,

                ContainerId = container.Id
            }),
            CommandFlags.FireAndForget
        );
    }
}