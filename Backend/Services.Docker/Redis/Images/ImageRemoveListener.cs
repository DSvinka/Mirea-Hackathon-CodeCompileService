using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Database;
using Services.Docker.Services;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Requests.Images;
using Services.Docker.Shared.Messages.Responses.Images;
using Services.Docker.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Docker.Redis.Images;

public class ImageRemoveListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerImageService _dockerImageService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ImageRemoveListener);
    
    public ImageRemoveListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerImageService dockerImageService)
    {
        _redisService = redisService;
        _dockerDbContext = dockerDbContext;
        _dockerImageService = dockerImageService;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{ListenerName} is starting.");

        stoppingToken.Register(() => _logger.LogInformation($"{ListenerName} is stopping."));

        var subscriber = _redisService.Connection.GetSubscriber();

        await subscriber.SubscribeAsync(DockerRedisChannels.ImageRemoveChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ImageRemoveChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ImageRemoveRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ImageRemoveChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        var image = await _dockerDbContext.DockerImages.FirstOrDefaultAsync(e => e.Id == request.ImageId);
        if (image == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ImageRemoveChannelResponse, 
                ErrorResponseGenerator.GetContainerNotFoundResponse(request.ConnectionId, ListenerName), 
                CommandFlags.FireAndForget
            );
        }
        
        if (image == null || !await _dockerImageService.ImageExistAsync(image.DockerImage, image.DockerImageTag))
        {
            if (image != null)
            {
                _dockerDbContext.DockerImages.Remove(image);
                await _dockerDbContext.SaveChangesAsync();

                await publisher.PublishAsync(
                    DockerRedisChannels.ImageRemoveChannelResponse,
                    JsonSerializer.Serialize(new ImageRemoveResponse
                    {
                        ConnectionId = request.ConnectionId,

                        ImageId = image.Id
                    }),
                    CommandFlags.FireAndForget
                );
                
                return;
            }
            else
            {
                // TODO: Вообще из-за такого рода операций лучше иметь сервер кеширования чтобы снять нагрузку с базы данных.
                // TODO: Но такие операции редкость, так что это можно оставить на будущее, когда нагрузка на сервис выростит, пока что времени на реализацию этого - нет.
                
                var checkImages = await _dockerImageService.ImageListAsync();
                if (checkImages != null && checkImages.Count >= 0)
                {
                    foreach (var checkImage in checkImages)
                    {
                        var imageName = checkImage.RepoTags[0].Split(':', StringSplitOptions.None)[0];
                        var imageTag = checkImage.RepoTags[0].Split(':', StringSplitOptions.None)[1];
                        // Если в базе данных нет такого контейнера - то его следует удалить, чтобы не было мусора.
                        if (!await _dockerDbContext.DockerImages.AnyAsync(e => e.DockerImage == imageName && e.DockerImageTag == imageTag))
                        {
                            await _dockerImageService.TryImageRemoveAsync(imageName, imageTag);
                        }
                    }
                }
                
                return;
            }
        }
        
        await _dockerImageService.TryImageRemoveAsync(image.DockerImage, image.DockerImageTag);
        
        _dockerDbContext.DockerImages.Remove(image);
        await _dockerDbContext.SaveChangesAsync();
        
        await publisher.PublishAsync(
            DockerRedisChannels.ImageRemoveChannelResponse,
            JsonSerializer.Serialize(new ImageRemoveResponse
            {
                ConnectionId = request.ConnectionId,

                ImageId = image.Id
            }),
            CommandFlags.FireAndForget
        );
    }
}