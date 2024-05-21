using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.Docker.Shared.Models;

namespace Services.Docker.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection UseDatabase(this IServiceCollection services, SettingsModel settings)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<DockerDbContext>(options => {
            options.UseNpgsql(
                $"Host={settings.PostgresHost};Port={settings.PostgresPort};Database={settings.PostgresDatabase};Username={settings.PostgresUsername};Password={settings.PostgresPassword}",
                serverOptions => serverOptions.MigrationsAssembly(typeof(DockerDbContext).Assembly.FullName)
            );
            options.UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
}