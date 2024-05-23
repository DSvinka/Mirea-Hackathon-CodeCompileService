using System.Text.Json;
using API.Auth.Services;
using API.Database;
using API.Shared;
using API.Shared.Queries.Responses;
using API.Shared.Queries.Responses.Docker.Containers;
using API.Shared.Queries.Responses.Docker.Images;
using Microsoft.AspNetCore.SignalR;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Responses.Containers;
using Services.Docker.Shared.Messages.Responses.Images;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace API.Hubs;

public class ImageDockerHub: Hub
{
    private readonly JwtService _jwtService;
    private readonly AuthDbContext _authDbContext;

    private readonly ISubscriber _subscriber;
    
    
    public ImageDockerHub(JwtService jwtService, AuthDbContext authDbContext, RedisService redisService)
    {
        _jwtService = jwtService;
        _authDbContext = authDbContext;

        _subscriber = redisService.Connection.GetSubscriber();
        
        // LIST
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ImageListChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ImageListResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                if (!user.IsAdministrator)
                {
                    var responses = new List<DockerImageUserResponse>(response.Images.Count);
                    responses.AddRange(
                        response.Images.Select(image => new DockerImageUserResponse
                        {
                            Id = image.Id,
                            
                            DisplayName = image.DisplayName,
                            Description = image.Description,
                            
                            CodeFileExtension = image.CodeFileExtension,
                            CodeEditorLang = image.CodeEditorLang,
                            
                            MaxCountByUser = image.MaxCountByUser
                        })
                    );

                    await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.ContainersUpdate, responses);
                }
                else
                {
                    var responses = new List<DockerImageAdminResponse>(response.Images.Count);
                    responses.AddRange(
                        response.Images.Select(image => new DockerImageAdminResponse
                        {
                            Id = image.Id,
                            
                            DockerImage = image.DockerImage,
                            DockerImageTag = image.DockerImageTag,
                            
                            DisplayName = image.DisplayName,
                            Description = image.Description,
                            
                            CodeFileExtension = image.CodeFileExtension,
                            CodeEditorLang = image.CodeEditorLang,
                            
                            MaxCountByUser = image.MaxCountByUser,
                            MaxCountByServer = image.MaxCountByServer,
                                
                            MaxMemory = image.MaxMemory,
                            MaxCpuShares = image.MaxCpuShares,
                            MaxStorage = image.MaxStorage,
                            
                            CodeInitCommand = image.CodeInitCommand,
                            CodeStartCommand = image.CodeStartCommand
                        })
                    );

                    await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.ContainersUpdate, responses);
                }
            }
        );
        
        
        // CREATE
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ImageAddChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ImageAddResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.Success, 
                    JsonSerializer.Serialize(new DockerImageAddResponse
                    {
                        ImageId = response.ImageId
                    })
                );
            }
        );
        
        
        // DELETE
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ImageRemoveChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ImageRemoveResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.Success, 
                    JsonSerializer.Serialize(new DockerImageRemoveResponse()
                    {
                        ImageId = response.ImageId
                    })
                );
            }
        );
        
        
        // ERROR
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ImageErrorChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<global::Shared.Utils.Redis.Messages.Responses.ErrorResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.Error, 
                    JsonSerializer.Serialize(new ErrorResponse
                    {
                        Message = response.Message,
                        Code = response.Code,
                        Data = response.Data
                    })
                );
            }
        );
    }

    
    public override async Task OnConnectedAsync()
    {
        var context = Context.GetHttpContext();
        if (context is not null)
        {
            var user = await _jwtService.GetUserAsync(context.User);
            if (user != null)
            {
                user.ConnectionId = context.Connection.Id;
                _authDbContext.Users.Update(user);
                await _authDbContext.SaveChangesAsync();
                
                await base.OnConnectedAsync();
                return;
            }
            
            context.Abort();
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var context = Context.GetHttpContext();
        if (context is not null)
        {
            var user = await _jwtService.GetUserAsync(context.User);
            if (user != null)
            {
                user.ConnectionId = null;
                _authDbContext.Users.Update(user);
                await _authDbContext.SaveChangesAsync();
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}