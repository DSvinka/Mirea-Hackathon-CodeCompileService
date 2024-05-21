namespace Services.Docker.Shared.Messages.Responses.Containers;

public class ContainerListResponse
{
    public required string ConnectionId;
    
    public required List<ContainerResponse> Containers;
}