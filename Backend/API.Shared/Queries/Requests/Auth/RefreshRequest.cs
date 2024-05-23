namespace API.Shared.Queries.Requests.Auth;

[Serializable]
public class RefreshRequest
{
    public required string RefreshToken;
}