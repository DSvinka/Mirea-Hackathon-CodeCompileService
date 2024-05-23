namespace API.Shared.Queries.Responses;

[Serializable]
public class ErrorResponse
{
    public required string Message;
    public string? Code;
    
    public Dictionary<string, string>? Data;


    public ErrorResponse WithCode(string serviceName, string codeName)
    {
        Code = $"{nameof(serviceName).ToLower()}.{codeName}";

        return this;
    }
}