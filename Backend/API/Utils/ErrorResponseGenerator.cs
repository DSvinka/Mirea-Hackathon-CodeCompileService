using System.Text.Json;
using Shared.Utils;
using Shared.Utils.Redis.Enums;
using Shared.Utils.Redis.Messages.Responses;
using StackExchange.Redis;

namespace API.Utils;

public static class ErrorResponseGenerator
{
    public static string GetUserNotFoundResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.Database,
            ErrorReasonType = EErrorReasonType.NotFound,
                
            Message = "Account not found!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.UserNotFound);;
            
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        return responseJson;
    }
    
    
    public static string GetAccessTokenIncorrectResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.MicroService,
            ErrorReasonType = EErrorReasonType.NotAuthorized,
                
            Message = "Access token is incorrect!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.AccessTokenIsIncorrect);
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }
    
    public static string GetRefreshTokenIsExpireResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.MicroService,
            ErrorReasonType = EErrorReasonType.IncorrectCredentials,
                
            Message = "Refresh token is expire!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.RefreshTokenIsExpire);
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }
    
    public static string GetIncorrectRequestResponse(string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ErrorAppType = EErrorAppType.MicroService,
            ErrorReasonType = EErrorReasonType.IncorrectRequest,
                
            Message = "Incorrect request, Serialization is failed!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        };
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }

    
    public static RedisValue GetEmailIsAlreadyExistResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.Database,
            ErrorReasonType = EErrorReasonType.AlreadyExist,
                
            Message = "User with this Email is already exist!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.EmailIsAlreadyUse);
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }
    
    public static RedisValue GetPhoneIsAlreadyExistResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.Database,
            ErrorReasonType = EErrorReasonType.AlreadyExist,
                
            Message = "User with this Phone is already exist!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.PhoneIsAlreadyUse);
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }
    
    
    public static string GetPasswordIsWrongResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.MicroService,
            ErrorReasonType = EErrorReasonType.IncorrectCredentials,
                
            Message = "Password is wrong!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.LoginOrPasswordIsIncorrect);
            
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        return responseJson;
    }
}