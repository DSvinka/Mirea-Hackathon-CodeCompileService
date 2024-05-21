using Shared.Utils.Redis.Enums;

namespace Shared.Utils.Redis.Messages.Responses;

[Serializable]
public class ErrorResponse
{
    public string? ConnectionId;

    public required EErrorAppType ErrorAppType;
    public required EErrorReasonType ErrorReasonType;

    public string? Trace;
    public required string Message;
    public required string Sender;
    
    public string? Code;
    
    public Dictionary<string, string>? Data;


    public ErrorResponse WithCode(string serviceName, string codeName)
    {
        Code = $"{nameof(serviceName).ToLower()}.{codeName}";

        return this;
    }
}