namespace API.Shared.Queries.Requests.Docker.Images;

public class DockerImageRemoveRequest
{
    public required string ConnectionId;
    
    public required long ImageId;
}