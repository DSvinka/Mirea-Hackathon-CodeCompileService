namespace Services.Auth.Shared.Messages.Requests;

[Serializable]
public class RegisterRequest
{
    public required string ConnectionId;
    
    public required string FirstName;
    public required string LastName;
    
    public required string Email;
    public string? Phone;

    public required string Password;

}