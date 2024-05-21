using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Services.Auth.Database;
using Services.Auth.Services;
using Services.Auth.Shared;
using Services.Auth.Shared.Messages.Requests;
using Services.Auth.Shared.Messages.Responses;
using Services.Auth.Utils;
using Shared.Utils;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace Services.Auth.Redis;

public class RefreshListener: BackgroundService
{
    private readonly AuthDbContext _authDbContext;
    private readonly JwtService _jwtService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(RefreshListener);
    
    public RefreshListener(RedisService redisService, LoggerFactory loggerFactory, JwtService jwtService, AuthDbContext authDbContext)
    {
        _redisService = redisService;
        _jwtService = jwtService;
        _authDbContext = authDbContext;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{ListenerName} is starting.");

        stoppingToken.Register(() => _logger.LogInformation($"{ListenerName} is stopping."));

        var subscriber = _redisService.Connection.GetSubscriber();

        await subscriber.SubscribeAsync(AuthRedisChannels.RefreshChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{AuthRedisChannels.RefreshChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<RefreshRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.RefreshChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            return;
        }
        
        var user = await _authDbContext.Users.FirstOrDefaultAsync(model => model.RefreshToken != null && model.RefreshToken.Equals(request.RefreshToken));
        if (user == null)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.RefreshChannelResponse, 
                ErrorResponseGenerator.GetUserNotFoundResponse(request.ConnectionId, ListenerName, new Dictionary<string, string>()
                {
                    {"refresh-token", request.RefreshToken}, {"connection-id", request.ConnectionId}
                }), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        if (user.RefreshTokenExpire < DateTime.UtcNow)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.RefreshChannelResponse, 
                ErrorResponseGenerator.GetRefreshTokenIsExpireResponse(request.ConnectionId, ListenerName, new Dictionary<string, string>()
                {
                    {"refresh-token", request.RefreshToken}, {"connection-id", request.ConnectionId}
                }), 
                CommandFlags.FireAndForget
            );
            
            return;
        }
        
        user.RefreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshTokenExpire = _jwtService.GetRefreshTokenExpire();
        
        var response = new AuthResponse()
        {
            ConnectionId = request.ConnectionId,

            AccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email),
            RefreshToken = user.RefreshToken
        };

        _authDbContext.Users.Update(user);
        await _authDbContext.SaveChangesAsync();
        
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        await publisher.PublishAsync(AuthRedisChannels.RefreshChannelResponse, responseJson, CommandFlags.FireAndForget);
    }
}