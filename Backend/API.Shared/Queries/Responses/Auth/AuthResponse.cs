namespace API.Shared.Queries.Responses.Auth;

[Serializable]
public class AuthResponse
{
    public required string AccessToken;
    public required string RefreshToken;
}