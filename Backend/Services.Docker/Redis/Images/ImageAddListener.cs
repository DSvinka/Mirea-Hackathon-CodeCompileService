using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Database;
using Services.Docker.Database.Models;
using Services.Docker.Services;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Requests.Images;
using Services.Docker.Shared.Messages.Responses.Images;
using Services.Docker.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Docker.Redis.Images;

public class ImageAddListener: BackgroundService
{
    private readonly DockerDbContext _dockerDbContext;
    private readonly DockerImageService _dockerImageService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ImageAddListener);
    
    public ImageAddListener(RedisService redisService, LoggerFactory loggerFactory, DockerDbContext dockerDbContext, DockerImageService dockerImageService)
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

        await subscriber.SubscribeAsync(DockerRedisChannels.ImageAddChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{DockerRedisChannels.ImageAddChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<ImageAddRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ImageErrorChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        if (await _dockerDbContext.DockerImages.AnyAsync(e => e.DockerImage == request.Image && e.DockerImageTag == request.ImageTag))
        {
            await publisher.PublishAsync(
                DockerRedisChannels.ImageErrorChannelResponse, 
                ErrorResponseGenerator.GetImageAlreadyExist(request.ConnectionId, ListenerName), 
                CommandFlags.FireAndForget
            );
        }
        
        await _dockerImageService.TryImageAddAsync(request.Image, request.ImageTag);
        var image = new DockerImageModel
        {
            DisplayName = request.DisplayName,
            Description = request.Description,
            
            DockerImage = request.Image,
            DockerImageTag = request.ImageTag,

            CodeFileExtension = request.CodeFileExtension,
            CodeEditorLang = request.CodeEditorLang,

            CodeInitCommand = request.CodeInitCommand,
            CodeStartCommand = request.CodeStartCommand,

            MaxMemory = request.MaxMemory,
            MaxCpuShares = request.MaxCpuShares,
            MaxStorage = request.MaxStorage,

            MaxCountByUser = request.MaxCountByUser,
            MaxCountByServer = request.MaxCountByServer,
        };

        await _dockerDbContext.DockerImages.AddAsync(image);
        await _dockerDbContext.SaveChangesAsync();
        await _dockerDbContext.Entry(image).GetDatabaseValuesAsync();
        
        await publisher.PublishAsync(
            DockerRedisChannels.ImageAddChannelResponse,
            JsonSerializer.Serialize(new ImageAddResponse()
            {
                ConnectionId = request.ConnectionId,

                ImageId = image.Id
            }),
            CommandFlags.FireAndForget
        );
    }
}