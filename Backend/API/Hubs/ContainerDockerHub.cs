using System.Text.Json;
using API.Auth.Services;
using API.Database;
using API.Shared;
using API.Shared.Queries.Responses;
using API.Shared.Queries.Responses.Docker.Containers;
using Microsoft.AspNetCore.SignalR;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Responses.Containers;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace API.Hubs;

public class ContainerDockerHub: Hub
{
    private readonly JwtService _jwtService;
    private readonly AuthDbContext _authDbContext;

    private readonly ISubscriber _subscriber;
    
    
    public ContainerDockerHub(JwtService jwtService, AuthDbContext authDbContext, RedisService redisService)
    {
        _jwtService = jwtService;
        _authDbContext = authDbContext;

        _subscriber = redisService.Connection.GetSubscriber();
        
        // LIST
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ContainerListChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ContainerListResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                if (!user.IsAdministrator)
                {
                    var responses = new List<DockerContainerUserResponse>(response.Containers.Count);
                    responses.AddRange(
                        response.Containers.Select(container => new DockerContainerUserResponse
                        {
                            Id = container.Id,
                            
                            UserId = container.UserId,
                            ContainerId = container.ContainerId,

                            Status = container.Status,

                            UsageMemory = container.UsageMemory,
                            UsageCpu = container.UsageCpu,
                            UsageStorage = container.UsageStorage,
                            
                            ProgramCode = container.ProgramCode
                        })
                    );

                    await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.ContainersUpdate, responses);
                }
                else
                {
                    var responses = new List<DockerContainerAdminResponse>(response.Containers.Count);
                    responses.AddRange(
                        response.Containers.Select(container => new DockerContainerAdminResponse()
                        {
                            Id = container.Id,
                            
                            UserId = container.UserId,
                            ContainerId = container.ContainerId,

                            Status = container.Status,

                            UsageMemory = container.UsageMemory,
                            UsageCpu = container.UsageCpu,
                            UsageStorage = container.UsageStorage,
                            
                            ProgramCode = container.ProgramCode,
                            ProgramCodeFolder = container.ProgramCodeFolder
                        })
                    );

                    await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.ContainersUpdate, responses);
                }
            }
        );
        
        
        // STATUS
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ContainerStatusChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ContainerResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                if (!user.IsAdministrator)
                {
                    var formatResponse = new DockerContainerUserResponse
                    {
                        Id = response.Id,
                        
                        UserId = response.UserId,
                        ContainerId = response.ContainerId,

                        Status = response.Status,
                        Logs = response.Logs,

                        UsageMemory = response.UsageMemory,
                        UsageCpu = response.UsageCpu,
                        UsageStorage = response.UsageStorage,
                        
                        ProgramCode = response.ProgramCode
                    };

                    await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.ContainersUpdate, formatResponse);
                }
                else
                {
                    var formatResponse = new DockerContainerAdminResponse()
                    {
                        Id = response.Id,
                        
                        UserId = response.UserId,
                        ContainerId = response.ContainerId,

                        Status = response.Status,
                        Logs = response.Logs,

                        UsageMemory = response.UsageMemory,
                        UsageCpu = response.UsageCpu,
                        UsageStorage = response.UsageStorage,
                        
                        ProgramCode = response.ProgramCode,
                        ProgramCodeFolder = response.ProgramCodeFolder
                    };

                    await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.ContainersUpdate, formatResponse);
                }
            }
        );
        
        
        // CREATE
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ContainerCreateAndRunChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ContainerCreateAndRunResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.Success, 
                    JsonSerializer.Serialize(new DockerContainerActionResponse()
                    {
                        Action = "create",
                        ContainerId = response.ContainerId
                    })
                );
            }
        );
        
        
        // DELETE
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ContainerDeleteAndStopChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<ContainerDeleteAndStopResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.Success, 
                    JsonSerializer.Serialize(new DockerContainerActionResponse()
                    {
                        Action = "delete",
                        ContainerId = response.ContainerId
                    })
                );
            }
        );
        
        
        // ERROR
        _subscriber.SubscribeAsync(
            DockerRedisChannels.ContainerErrorChannelResponse, 
            async (channel, value) =>
            {
                var response = JsonSerializer.Deserialize<global::Shared.Utils.Redis.Messages.Responses.ErrorResponse>(value);
                var user = await _jwtService.GetUserAsync(response.ConnectionId);
                
                await Clients.Client(response.ConnectionId).SendAsync(DockerHubMethods.Success, 
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