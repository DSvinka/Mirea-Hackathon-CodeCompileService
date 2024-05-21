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

public class VerifyListener: BackgroundService
{
    private readonly AuthDbContext _authDbContext;
    private readonly JwtService _jwtService;
    private readonly RedisService _redisService;
    private readonly ILogger _logger;

    public const string ListenerName = nameof(VerifyListener);
    
    public VerifyListener(RedisService redisService, LoggerFactory loggerFactory, JwtService jwtService, AuthDbContext authDbContext)
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

        await subscriber.SubscribeAsync(AuthRedisChannels.VerifyChannelRequest, Receive);
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogDebug($"{ListenerName} is stopping.");
    }

    public async void Receive(RedisChannel channel, RedisValue message)
    {
        _logger.LogInformation($"{AuthRedisChannels.VerifyChannelRequest} -> {message}");
        
        var publisher = _redisService.Connection.GetSubscriber();
        
        var request = JsonSerializer.Deserialize<VerifyRequest>(message, JsonOptions.Options);
        if (request == null)
        {
            await publisher.PublishAsync(
                AuthRedisChannels.VerifyChannelResponse, 
                ErrorResponseGenerator.GetIncorrectRequestResponse(ListenerName), 
                CommandFlags.FireAndForget
            );
            return;
        }
        
        var response = new VerifyResponse()
        {
            ConnectionId = request.ConnectionId,
            
            IsAdministrator = false,
            IsAuthorized = false
        };
        
        var userIdString = _jwtService.GetUserIdFromAccessToken(request.AccessToken);
        if (userIdString != null)
        {
            var userId = int.Parse(userIdString);
            var user = await _authDbContext.Users.FirstOrDefaultAsync(user => user.Id == userId);
            
            if (user != null)
            {
                response.IsAuthorized = true;
                response.IsAdministrator = user.IsAdministrator;
            }
        }
        
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        await publisher.PublishAsync(AuthRedisChannels.VerifyChannelResponse, responseJson, CommandFlags.FireAndForget);
    }
}