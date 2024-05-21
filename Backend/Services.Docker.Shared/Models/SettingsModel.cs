namespace Services.Docker.Shared.Models;

public class SettingsModel
{
    public required string PostgresHost;
    public required string PostgresPort;
    public required string PostgresDatabase;
    public required string PostgresUsername;
    public required string PostgresPassword;

    public required string RedisHost;
    public required string RedisPassword;

    public required string DockerHost;
}