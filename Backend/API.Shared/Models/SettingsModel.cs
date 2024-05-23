using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Shared.Models;

public class SettingsModel
{
    public required string HashSecretKey;
    public required string JwtSecretKey;
    
    public required string JwtIssuer;
    public required string JwtAudience;
    
    public required float JwtAccessTokenLifeMinutes;
    public required float JwtRefreshTokenLifeMinutes;

    public required string PostgresHost;
    public required string PostgresPort;
    public required string PostgresDatabase;
    public required string PostgresUsername;
    public required string PostgresPassword;

    public required string RedisHost;
    public required string RedisPassword;

    public SymmetricSecurityKey GetJwtSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
}