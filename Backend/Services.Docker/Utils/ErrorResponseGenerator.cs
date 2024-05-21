using System.Text.Json;
using Shared.Utils;
using Shared.Utils.Redis.Enums;
using Shared.Utils.Redis.Messages.Responses;
using StackExchange.Redis;

namespace Services.Docker.Utils;

public static class ErrorResponseGenerator
{
    public static RedisValue GetImageAlreadyExist(string? connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.Docker,
            ErrorReasonType = EErrorReasonType.AlreadyExist,
                
            Message = "This Docker Image already exist!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.DockerImageAlreadyExist);;
            
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        return responseJson;
    }
    
    public static string GetImageNotFoundResponse(string? connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.Docker,
            ErrorReasonType = EErrorReasonType.NotFound,
                
            Message = "Docker Image not found!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.DockerImageNotFound);;
            
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        return responseJson;
    }
    
    public static string GetContainerNotFoundResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
                
            ErrorAppType = EErrorAppType.Docker,
            ErrorReasonType = EErrorReasonType.NotFound,
                
            Message = "Docker Image not found!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.DockerContainerNotFound);
            
        var responseJson = JsonSerializer.Serialize(response, JsonOptions.Options);
        return responseJson;
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

    public static string GetToManyServerContainersResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
            
            ErrorAppType = EErrorAppType.Docker,
            ErrorReasonType = EErrorReasonType.ToManyExist,
                
            Message = "Server is overloaded! To many containers for this programing language!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.DockerContainersServerLimitReached);
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }
    
    public static string GetToManyUserContainersResponse(string connectionId, string listenerName, Dictionary<string, string>? data = null)
    {
        var response = new ErrorResponse
        {
            ConnectionId = connectionId,
            
            ErrorAppType = EErrorAppType.Docker,
            ErrorReasonType = EErrorReasonType.ToManyExist,
                
            Message = "User limit reached! To many containers for this programing language!",
            Sender = $"{typeof(ErrorResponseGenerator).Assembly.FullName}-{listenerName}",
                
            Data = data
        }.WithCode(typeof(ErrorResponseGenerator).Assembly.FullName, ErrorCodeNames.DockerContainersUserLimitReached);;
            
        return JsonSerializer.Serialize(response, JsonOptions.Options);
    }
}