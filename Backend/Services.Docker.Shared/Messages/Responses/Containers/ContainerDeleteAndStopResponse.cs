namespace Services.Docker.Shared.Messages.Responses.Containers;

public class ContainerDeleteAndStopResponse
{
    public required string ConnectionId;

    public required long ContainerId;
}