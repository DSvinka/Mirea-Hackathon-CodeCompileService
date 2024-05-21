namespace Services.Auth.Shared.Messages.Responses;

[Serializable]
public class VerifyResponse
{
    public required string ConnectionId;
    
    public required bool IsAuthorized;
    public required bool IsAdministrator;
}