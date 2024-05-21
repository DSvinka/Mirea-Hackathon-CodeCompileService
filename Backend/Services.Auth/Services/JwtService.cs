using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Services.Auth.Shared.Models;

namespace Services.Auth.Services;

public class JwtService 
{
    private readonly SettingsModel _settingsModel;

    public JwtService(SettingsModel settingsModel)
    {
        _settingsModel = settingsModel;
    }

    public string GenerateAccessToken(long userId, string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, email)
        };
        
        var jwt = new JwtSecurityToken(
            issuer: _settingsModel.JwtIssuer,
            audience: _settingsModel.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(_settingsModel.JwtAccessTokenLifeMinutes)),
            signingCredentials: new SigningCredentials(_settingsModel.GetJwtSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }


    public string? GetUserIdFromAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var validations = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _settingsModel.GetJwtSymmetricSecurityKey(),
            
            ValidateLifetime = true,
            ValidateIssuer = false,
            ValidateAudience = true
        };
        
        var claims = handler.ValidateToken(token, validations, out var tokenSecure);
        
        return claims
            ?.Claims.FirstOrDefault(claim => claim.ValueType == ClaimTypes.NameIdentifier)
            ?.Value;
    }

    public DateTime GetRefreshTokenExpire()
    {
        return DateTime.UtcNow.AddMinutes(_settingsModel.JwtRefreshTokenLifeMinutes);
    }
}