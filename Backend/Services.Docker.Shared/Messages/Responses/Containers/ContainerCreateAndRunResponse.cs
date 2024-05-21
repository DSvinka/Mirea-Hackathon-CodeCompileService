namespace Services.Docker.Shared.Messages.Responses.Containers;

public class ContainerCreateAndRunResponse
{
    public required string ConnectionId;

    public required string ContainerId;
}