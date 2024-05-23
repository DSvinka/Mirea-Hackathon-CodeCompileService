namespace API.Shared.Queries.Requests.Auth;

[Serializable]
public class RegisterRequest
{
    public required string FirstName;
    public required string LastName;
    
    public required string Email;
    public string? Phone;

    public required string Password;

}