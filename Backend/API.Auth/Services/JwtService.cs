using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using API.Database;
using API.Database.Models;
using API.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Auth.Services;

public class JwtService 
{
    private readonly SettingsModel _settingsModel;
    private readonly AuthDbContext _authDbContext;

    public JwtService(SettingsModel settingsModel, AuthDbContext authDbContext)
    {
        _settingsModel = settingsModel;
        _authDbContext = authDbContext;
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


    public async Task<UserModel?> GetUserAsync(ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(x => x.Type == "id");
        if (userIdClaim == null)
        {
            return null;
        }
        
        var userModel = await _authDbContext.Users.FirstOrDefaultAsync(e => e.Id == long.Parse(userIdClaim.Value));
        if (userModel == null)
        {
            return null;
        }

        return userModel;
    }
    
    public async Task<UserModel?> GetUserAsync(string connectionId)
    {
        var userModel = await _authDbContext.Users.FirstOrDefaultAsync(e => e.ConnectionId != null && e.ConnectionId == connectionId);
        if (userModel == null)
        {
            return null;
        }

        return userModel;
    }

    public DateTime GetRefreshTokenExpire()
    {
        return DateTime.UtcNow.AddMinutes(_settingsModel.JwtRefreshTokenLifeMinutes);
    }
}