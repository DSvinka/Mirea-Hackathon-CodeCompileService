namespace API.Shared.Queries.Responses.Auth;

[Serializable]
public class VerifyResponse
{
    public required bool IsAuthorized;
    public required bool IsAdministrator;
}