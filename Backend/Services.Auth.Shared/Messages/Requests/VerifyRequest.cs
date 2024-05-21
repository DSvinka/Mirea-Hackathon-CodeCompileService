namespace Services.Auth.Shared.Messages.Requests;

[Serializable]
public class VerifyRequest
{
    public required string ConnectionId;
    
    public required string AccessToken;
}