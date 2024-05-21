namespace Services.Auth.Shared.Messages.Responses;

[Serializable]
public class ProfileResponse
{
    public required string ConnectionId;
    
    public required string FirstName;
    public required string LastName;

    public required string Email;
    public string? Phone;
    
    public required bool IsAdministrator;
}