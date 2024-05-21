namespace Services.Docker.Shared.Messages.Requests.Containers;

public class ContainerStatusRequest
{
    public required string ConnectionId;
    
    public required string ContainerId;
    public long? CheckUserId;
}