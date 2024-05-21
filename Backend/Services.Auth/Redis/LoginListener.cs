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

public class LoginListener: BackgroundService
{
    private readonly JwtService _jwtService;
    private readonly HashService _hashService;
    private readonly AuthDbContext _authDbContext;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(ProfileListener);
    
    public LoginListener(RedisService redisService, LoggerFactory loggerFactory, AuthDbContext authDbContext, JwtService jwtService, HashService hashService)
    {
        _redisService = redisService;
        _authDbContext = authDbContext;
        _jwtService = jwtService;
        _hashService = hashService;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{ListenerName} is starting.");

        stoppingToken.Register(() => _logger.LogInformation($"{ListenerName} is stopping."));

        var subscriber = _redisService.Connection.GetSubscriber();

        await subscriber.SubscribeAsync(AuthRedisChannels.LoginChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{AuthRedisChannels.LoginChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<LoginRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.LoginChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        var user = await _authDbContext.Users.FirstOrDefaultAsync(model => model.Email == request.Email);
        if (user == null)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.LoginChannelResponse, 
                ErrorResponseGenerator.GetUserNotFoundResponse(request.ConnectionId, ListenerName, new Dictionary<string, string>()
                {
                    {"email", request.Email}, {"connection-id", request.ConnectionId}
                }), 
                CommandFlags.FireAndForget
            );
            
            return;
        }

        if (!_hashService.EqualsHash(request.Password, user.PasswordHash))
        {
            await publisher.PublishAsync(
                AuthRedisChannels.LoginChannelResponse, 
                ErrorResponseGenerator.GetPasswordIsWrongResponse(request.ConnectionId, ListenerName, new Dictionary<string, string>()
                {
                    {"email", request.Email}, {"connection-id", request.ConnectionId}
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
        await publisher.PublishAsync(AuthRedisChannels.LoginChannelResponse, responseJson, CommandFlags.FireAndForget);
    }
}