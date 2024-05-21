namespace Services.Auth.Shared.Messages.Responses;

[Serializable]
public class AuthResponse
{
    public required string ConnectionId;
    
    public required string AccessToken;
    public required string RefreshToken;
}