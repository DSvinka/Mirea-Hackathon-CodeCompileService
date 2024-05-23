namespace API.Shared.Queries.Requests.Auth;

[Serializable]
public class LoginRequest
{
    public required string Email;
    public required string Password;
}