namespace Services.Docker.Shared.Messages.Requests.Containers;

public class ContainerDeleteAndStopRequest
{
    public required string ConnectionId;
    
    public required long ContainerId;
    public long? CheckUserId;
}