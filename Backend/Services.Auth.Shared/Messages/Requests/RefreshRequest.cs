namespace Services.Auth.Shared.Messages.Requests;

[Serializable]
public class RefreshRequest
{
    public required string ConnectionId;
    
    public required string RefreshToken;
}