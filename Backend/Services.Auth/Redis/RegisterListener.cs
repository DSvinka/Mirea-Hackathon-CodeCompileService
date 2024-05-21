using System.Text.Json;
using Services.Auth.Database;
using Services.Auth.Database.Models;
using Services.Auth.Services;
using Services.Auth.Shared;
using Services.Auth.Shared.Messages.Requests;
using Services.Auth.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Auth.Redis;

public class RegisterListener: BackgroundService
{
    private readonly HashService _hashService;
    private readonly AuthDbContext _authDbContext;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ProfileListener);
    
    public RegisterListener(RedisService redisService, LoggerFactory loggerFactory, AuthDbContext authDbContext, HashService hashService)
    {
        _redisService = redisService;
        _authDbContext = authDbContext;
        _hashService = hashService;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{ListenerName} is starting.");

        stoppingToken.Register(() => _logger.LogInformation($"{ListenerName} is stopping."));

        var subscriber = _redisService.Connection.GetSubscriber();

        await subscriber.SubscribeAsync(AuthRedisChannels.RegisterChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{AuthRedisChannels.RegisterChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<RegisterRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.RegisterChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        if (_authDbContext.Users.Any(user => user.Email == request.Email))
        {
            await publisher.PublishAsync(
                AuthRedisChannels.RegisterChannelResponse,
                ErrorResponseGenerator.GetEmailIsAlreadyExistResponse(request.ConnectionId, ListenerName, new Dictionary<string, string>()
                {
                    {"email", request.Email}, {"connection-id", request.ConnectionId}   
                }),
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        if (request.Phone != null && _authDbContext.Users.Any(user => user.Phone == request.Phone))
        {
            await publisher.PublishAsync(
                AuthRedisChannels.RegisterChannelResponse,
                ErrorResponseGenerator.GetPhoneIsAlreadyExistResponse(request.ConnectionId, ListenerName, new Dictionary<string, string>()
                {
                    {"phone", request.Phone}, {"connection-id", request.ConnectionId}   
                }),
                CommandFlags.FireAndForget
            );
            
            return;
        }

        await _authDbContext.Users.AddAsync(new UserModel
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            
            Email = request.Email,
            Phone = request.Phone,
            
            IsAdministrator = false,
            PasswordHash = _hashService.GetHash(request.Password)
        });
        
        await _authDbContext.SaveChangesAsync();

    }
}