using Services.Auth.Shared.Models;

namespace Services.Auth.Services;

public class HashService
{
    private readonly SettingsModel _settings;

    public HashService(SettingsModel settings)
    {
        _settings = settings;
    }

    public string GetHash(string text)
    {
        return BCrypt.Net.BCrypt.HashPassword(text, _settings.HashSecretKey);
    }

    public bool EqualsHash(string text, string hashText)
    {
        return BCrypt.Net.BCrypt.Verify(text, hashText);
    }
}