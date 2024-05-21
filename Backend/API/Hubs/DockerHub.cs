using API.Shared;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class DockerHub: Hub
{
    public async Task SendContainersUpdate(string containers)
    {
        await Clients.All.SendAsync(DockerHubMethods.DockerContainersUpdate, message);
    }
    
    public async Task SendImagesUpdate(string images)
    {
        await Clients.All.SendAsync(DockerHubMethods.DockerImagesUpdate, message);
    }
    
    
    public async Task SendActionResponse(string message)
    {
        await Clients.All.SendAsync(DockerHubMethods.DockerActionResponse, message);
    }
}