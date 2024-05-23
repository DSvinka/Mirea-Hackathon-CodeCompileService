namespace API.Shared.Queries.Requests.Docker.Containers;

public class DockerContainerDeleteRequest
{
    public required string ConnectionId;
    
    public required long ContainerId;
}