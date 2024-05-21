using Services.Docker.Database;
using Services.Docker.Database.Models;
using Services.Docker.Redis.Containers;
using Services.Docker.Redis.Images;
using Services.Docker.Shared.Models;
using Shared.Utils.Redis.Services;

var builder = Host.CreateApplicationBuilder(args);

var settings = new SettingsModel
{
    PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST")!,
    PostgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT")!,
    PostgresDatabase = Environment.GetEnvironmentVariable("POSTGRES_DATABASE")!,
    PostgresUsername = Environment.GetEnvironmentVariable("POSTGRES_USERNAME")!,
    PostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!,

    RedisHost = Environment.GetEnvironmentVariable("REDIS_HOST")!,
    RedisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD")!,
    
    DockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST")!  // "unix:///var/run/docker.sock",
};

builder.Services.AddSingleton(settings);

builder.Services.AddSingleton<DockerContainerModel>();
builder.Services.AddSingleton<DockerImageModel>();

var redisService = new RedisService(settings.RedisHost, settings.RedisPassword);
builder.Services.AddSingleton(redisService);

builder.Services.UseDatabase(settings);

builder.Services.AddHostedService<ContainerCreateAndRunListener>();
builder.Services.AddHostedService<ContainerDeleteAndStopListener>();
builder.Services.AddHostedService<ContainerListListener>();
builder.Services.AddHostedService<ContainerStatusListener>();

builder.Services.AddHostedService<ImageAddListener>();
builder.Services.AddHostedService<ImageRemoveListener>();
builder.Services.AddHostedService<ImageListListener>();



var host = builder.Build();
host.Run();