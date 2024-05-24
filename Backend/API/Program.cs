using System.Text;
using API.Auth.Services;
using API.Database;
using API.Hubs;
using API.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.IdentityModel.Tokens;
using Shared.Utils.Redis.Services;

var builder = WebApplication.CreateBuilder(args);

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
    RedisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD"),
};

builder.Services.AddSingleton(settings);

builder.Services.AddSingleton<HashService>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy  =>
        {
            policy.WithOrigins(
                "https://dsvinka.ru", "https://compile.dsvinka.ru", 
                "http://localhost", "https://localhost", 
                "http://127.0.0.1", "https://127.0.0.1"
            );
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowCredentials();
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "compile.dsvinka.ru",
            ValidAudience = "compile.dsvinka.ru",
            IssuerSigningKey = settings.GetJwtSymmetricSecurityKey()
        };
    });
builder.Services.AddAuthorization();


var redisService = new RedisService(settings.RedisHost, settings.RedisPassword);
builder.Services.AddSingleton(redisService);

builder.Services.UseDatabase(settings);

builder.Services.AddSignalR(); 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ContainerDockerHub>("/ws/docker/containers", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});
app.MapHub<ImageDockerHub>("/ws/docker/images", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});


app.Run();