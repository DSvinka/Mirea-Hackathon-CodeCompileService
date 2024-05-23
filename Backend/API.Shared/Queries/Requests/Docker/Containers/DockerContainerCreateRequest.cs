namespace API.Shared.Queries.Requests.Docker.Containers;

public class DockerContainerCreateRequest
{
    public required string ConnectionId;

    public required long ImageId;
    
    public required string ProgramCode;
}