using API.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection UseDatabase(this IServiceCollection services, SettingsModel settings)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<AuthDbContext>(options => {
            options.UseNpgsql(
                $"Host={settings.PostgresHost};Port={settings.PostgresPort};Database={settings.PostgresDatabase};Username={settings.PostgresUsername};Password={settings.PostgresPassword}",
                serverOptions => serverOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName)
            );
            options.UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
}