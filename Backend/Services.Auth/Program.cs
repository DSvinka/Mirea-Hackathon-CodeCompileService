using Services.Auth.Database;
using Services.Auth.Redis;
using Services.Auth.Services;
using Services.Auth.Shared.Models;
using Shared.Utils.Redis.Services;

var builder = Host.CreateApplicationBuilder(args);

var settings = new SettingsModel
{
    HashSecretKey = Environment.GetEnvironmentVariable("HASH_SECRET_KEY")!,

    JwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!,
    JwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!,
    JwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!,
    JwtAccessTokenLifeMinutes = Convert.ToSingle(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFE_MINUTES")!),
    JwtRefreshTokenLifeMinutes = Convert.ToSingle(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_LIFE_MINUTES")!),

    PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST")!,
    PostgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT")!,
    PostgresDatabase = Environment.GetEnvironmentVariable("POSTGRES_DATABASE")!,
    PostgresUsername = Environment.GetEnvironmentVariable("POSTGRES_USERNAME")!,
    PostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!,
    
    RedisHost = Environment.GetEnvironmentVariable("REDIS_HOST")!,
    RedisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD")!,
};

builder.Services.AddSingleton(settings);

builder.Services.AddSingleton<HashService>();
builder.Services.AddSingleton<JwtService>();

var redisService = new RedisService(settings.RedisHost, settings.RedisPassword);
builder.Services.AddSingleton(redisService);

builder.Services.UseDatabase(settings);

builder.Services.AddHostedService<LoginListener>();
builder.Services.AddHostedService<RegisterListener>();
builder.Services.AddHostedService<RefreshListener>();
builder.Services.AddHostedService<VerifyListener>();
builder.Services.AddHostedService<ProfileListener>();

var host = builder.Build();
host.Run();