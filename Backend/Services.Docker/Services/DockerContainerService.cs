using Docker.DotNet;
using Docker.DotNet.Models;
using Services.Docker.Database.Models;

namespace Services.Docker.Services;

public class DockerContainerService
{
    private readonly DockerClient _client;

    public DockerContainerService(DockerClient client)
    {
        _client = client;
    }

    public async Task<string> ContainerCreateAsync(DockerImageModel image, string userId, CancellationToken cancellationToken = default)
    {
        var config = new Config
        {
            Image = $"{image.DockerImage}:{image.DockerImageTag}",
            Cmd = image.CodeInitCommand == null ? new List<string>() : image.CodeInitCommand.Split("//"),
            AttachStdout = true
        };

        var hostConfig = new HostConfig
        {
            Memory = image.MaxMemory * 1024 * 1024,
            CPUShares = image.MaxCpuShares,
            StorageOpt = new Dictionary<string, string>()
            {
                {"size", $"{image.MaxStorage}MB"}
            }
        };

        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters(config)
        {
            Name = $"runner_code__{userId}__{image.DockerImage}",
            HostConfig = hostConfig
        }, cancellationToken);

        var containerId = response.ID;
        await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters(), cancellationToken);

        return containerId;
    }
    
    public async Task<bool> TryContainerDeleteAsync(string containerId, CancellationToken cancellationToken = default)
    {
        if (!await ContainerExistAsync(containerId, cancellationToken))
            return false;
        
        await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters()
        {
            RemoveLinks = true,
            RemoveVolumes = true
        }, cancellationToken);

        return true;
    }
    
    
    public async Task<bool> TryContainerRunAsync(string containerId, CancellationToken cancellationToken = default)
    {
        if (!await ContainerExistAsync(containerId, cancellationToken))
            return false;
        
        await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters(), cancellationToken);
        return true;
    }
    
    public async Task<bool> TryContainerStopAsync(string containerId, CancellationToken cancellationToken = default)
    {
        if (!await ContainerExistAsync(containerId, cancellationToken))
            return false;
        
        await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), cancellationToken);
        return true;
    }
    
    public async Task<bool> TryContainerRestartAsync(string containerId, CancellationToken cancellationToken = default)
    {
        if (!await ContainerExistAsync(containerId, cancellationToken))
            return false;
        
        await _client.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters(), cancellationToken);
        return true;
    }
    
    
    public async Task<IList<ContainerListResponse>?> ContainerListAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                { "name", new Dictionary<string, bool> {{"runner_code__*", true}} }
            }
        }, cancellationToken);

        return response;
    }
    
    public async Task<IList<ContainerListResponse>?> ContainerListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                { "name", new Dictionary<string, bool> {{$"runner_code__{userId}__*", true}} }
            }
        }, cancellationToken);

        return response;
    }
    
    public async Task<IList<ContainerListResponse>?> ContainerListByImageAsync(string imageName, string imageTag, CancellationToken cancellationToken = default)
    {
        var response = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                { "ancestor", new Dictionary<string, bool> {{$"{imageName}:{imageTag}", true}} }
            }
        }, cancellationToken);

        return response;
    }
    
    
    public async Task<ContainerListResponse?> ContainerOneAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var response = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                { "id", new Dictionary<string, bool> {{containerId, true}} }
            }
        }, cancellationToken);

        if (response == null || response.Count == 0)
            return null;

        var container = response[0];
        return container;
    }


    public async Task<bool> ContainerExistAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var images = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                {"id", new Dictionary<string, bool>() {{containerId, true}}}
            }
        }, cancellationToken);
        
        if (images == null || images.Count == 0)
            return false;

        return true;
    }
}