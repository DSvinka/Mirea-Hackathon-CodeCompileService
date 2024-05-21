namespace Services.Docker.Shared.Messages.Requests.Containers;

public class ContainerListRequest
{
    public required string ConnectionId;
    
    public long? FilterUserId = null;
    public long? FilterImageId = null;
}