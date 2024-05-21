
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

public class ContainerDeleteAndStopListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerContainerService _dockerContainerService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ContainerDeleteAndStopListener);
    
    public ContainerDeleteAndStopListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerContainerService dockerContainerService)
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

        await subscriber.SubscribeAsync(DockerRedisChannels.ContainerDeleteAndStopChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ContainerDeleteAndStopChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ContainerDeleteAndStopRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerDeleteAndStopChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        bool containerNotFound = false;
        
        var container = await _dockerDbContext.DockerContainers.FirstOrDefaultAsync(e => e.Id == request.ContainerId);
        if (container == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ContainerDeleteAndStopChannelResponse, 
                ErrorResponseGenerator.GetContainerNotFoundResponse(request.ConnectionId, ListenerName), 
                CommandFlags.FireAndForget
            );

            containerNotFound = true;
        }

        var exist = await _dockerContainerService.ContainerExistAsync(container.ContainerId);
        if (!exist)
        {
            if (!containerNotFound)
            {
                _dockerDbContext.DockerContainers.Remove(container);
                await _dockerDbContext.SaveChangesAsync();

                await publisher.PublishAsync(
                    DockerRedisChannels.ContainerDeleteAndStopChannelResponse,
                    ErrorResponseGenerator.GetContainerNotFoundResponse(request.ConnectionId, ListenerName),
                    CommandFlags.FireAndForget
                );
                
                containerNotFound = true;
            }
            else
            {
                // TODO: Вообще из-за такого рода операций лучше иметь сервер кеширования чтобы снять нагрузку с базы данных.
                // TODO: Но такие операции редкость, так что это можно оставить на будущее, когда нагрузка на сервис выростит, пока что времени на реализацию этого - нет.
                
                var checkContainers = await _dockerContainerService.ContainerListAsync();
                if (checkContainers != null && checkContainers.Count >= 0)
                {
                    foreach (var checkContainer in checkContainers)
                    {
                        // Если в базе данных нет такого контейнера - то его следует удалить, чтобы не было мусора.
                        if (!await _dockerDbContext.DockerContainers.AnyAsync(e => e.ContainerId == checkContainer.ID))
                        {
                            await _dockerContainerService.TryContainerStopAsync(container.ContainerId);
                            await _dockerContainerService.TryContainerDeleteAsync(checkContainer.ID);
                        }
                    }
                }
            }
        }

        if (containerNotFound)
            return;
        
        await _dockerContainerService.TryContainerStopAsync(container.ContainerId);
        await _dockerContainerService.TryContainerDeleteAsync(container.ContainerId);

        _dockerDbContext.DockerContainers.Remove(container);
        await _dockerDbContext.SaveChangesAsync();
        
        await publisher.PublishAsync(
            DockerRedisChannels.ContainerDeleteAndStopChannelResponse,
            JsonSerializer.Serialize(new ContainerDeleteAndStopResponse()
            {
                ConnectionId = request.ConnectionId,

                ContainerId = request.ConnectionId
            }),
            CommandFlags.FireAndForget
        );
    }
}