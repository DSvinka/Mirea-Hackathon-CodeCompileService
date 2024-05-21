namespace Services.Auth.Shared.Messages.Requests;

[Serializable]
public class LoginRequest
{
    public required string ConnectionId;
    
    public required string Email;
    public required string Password;
}