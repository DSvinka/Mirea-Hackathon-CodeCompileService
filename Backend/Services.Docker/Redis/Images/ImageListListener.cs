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

public class ImageListListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerImageService _dockerImageService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ImageListListener);
    
    public ImageListListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerImageService dockerImageService)
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

        await subscriber.SubscribeAsync(DockerRedisChannels.ImageListChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ImageListChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ImageListRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ImageErrorChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        var images = await _dockerDbContext.DockerImages.ToListAsync();
        if (images.Count == 0)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ImageErrorChannelResponse,
                ErrorResponseGenerator.GetImageNotFoundResponse(request.ConnectionId, ListenerName),
                CommandFlags.FireAndForget
            );
            
            return;
        }

        var imageResponses = new List<ImageResponse>(images.Count);
        imageResponses.AddRange(
            images.Select(image => new ImageResponse
            {
                Id = image.Id,
                ConnectionId = request.ConnectionId,
                
                DisplayName = image.DisplayName,
                Description = image.Description,

                DockerImage = image.DockerImage,
                DockerImageTag = image.DockerImageTag,

                CodeFileExtension = image.CodeFileExtension,
                CodeEditorLang = image.CodeEditorLang,

                CodeInitCommand = image.CodeInitCommand,
                CodeStartCommand = image.CodeStartCommand,

                MaxMemory = image.MaxMemory,
                MaxCpuShares = image.MaxCpuShares,
                MaxStorage = image.MaxStorage,

                MaxCountByUser = image.MaxCountByUser,
                MaxCountByServer = image.MaxCountByServer,
            })
        );

        await publisher.PublishAsync(
            DockerRedisChannels.ImageListChannelResponse,
            JsonSerializer.Serialize(new ImageListResponse
            {
                ConnectionId = request.ConnectionId,

                Images = imageResponses
            })
        );
    }
}