using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using Services.Docker.Database.Models;

namespace Services.Docker.Services;

public class DockerContainerService
{
    public static readonly string CodeFolder = $"{Directory.GetCurrentDirectory()}/code";
    private readonly DockerClient _client;

    public DockerContainerService(DockerClient client)
    {
        _client = client;
    }

    public async Task<(string containerId, string folder)> ContainerCreateAsync(DockerImageModel image, string userId, string programCode, CancellationToken cancellationToken = default)
    {
        var folderName = Guid.NewGuid();
        var codeFile = $"{CodeFolder}/user-{userId}/{folderName}/code.{image.CodeFileExtension}";

        var stream = File.Create(codeFile);
        var writer = new System.IO.StreamWriter(stream);
        await writer.WriteAsync(programCode);
        writer.Close();
        await writer.DisposeAsync();
        
        var config = new Config
        {
            Image = $"{image.DockerImage}:{image.DockerImageTag}",
            Cmd = image.CodeInitCommand == null ? new List<string>() : image.CodeInitCommand.Split("//"),
            AttachStdout = true
        };

        var hostConfig = new HostConfig
        {
            Binds = new List<string>() {$"{Directory.GetCurrentDirectory()}/code/user-{userId}/{Guid.NewGuid()}:/app"},
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

        var container = await _client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters()
        {
            AttachStderr = true,
            AttachStdin = true,
            AttachStdout = true,
            Cmd = image.CodeStartCommand.Replace("{file}", $"code.{image.CodeFileExtension}").Split("//")
        }, cancellationToken);
        
        return (container.ID, folderName.ToString());
    }
    
    public async Task<bool> TryContainerDeleteAsync(DockerContainerModel container, CancellationToken cancellationToken = default)
    {
        if (!await ContainerExistAsync(container.ContainerId, cancellationToken))
            return false;
        
        File.Delete($"{CodeFolder}/user-{container.UserId}/{container.ProgramCodeFolder}/code.{container.DockerImage.CodeFileExtension}");
        
        await _client.Containers.RemoveContainerAsync(container.ContainerId, new ContainerRemoveParameters()
        {
            RemoveLinks = true,
            RemoveVolumes = true
        }, cancellationToken);

        return true;
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

    public async Task<string> ContainerLogsAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var log = "";
        
        var logStream = await _client.Containers.GetContainerLogsAsync(containerId, false, new ContainerLogsParameters(), cancellationToken);
        if (logStream != null)
        { 
            (log, _) = await logStream.ReadOutputToEndAsync(cancellationToken);
        }
        
        return log;
    }
    
    public async Task<ContainerStatsResponse> ContainerStatsAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<ContainerStatsResponse>();

        var progress = new Progress<ContainerStatsResponse>(stats => tcs.TrySetResult(stats));

        try
        {
            await _client.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters(), progress,
                cancellationToken);
            return await tcs.Task;
        }
        catch (Exception e)
        {
            tcs.TrySetException(e);
            throw;
        }
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