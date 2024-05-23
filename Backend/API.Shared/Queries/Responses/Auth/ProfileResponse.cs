namespace API.Shared.Queries.Responses.Auth;

[Serializable]
public class ProfileResponse
{
    public required string FirstName;
    public required string LastName;

    public required string Email;
    public string? Phone;
    
    public required bool IsAdministrator;
}