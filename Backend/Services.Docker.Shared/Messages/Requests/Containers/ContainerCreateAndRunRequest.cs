namespace Services.Docker.Shared.Messages.Requests.Containers;

public class ContainerCreateAndRunRequest
{
    public required string ConnectionId;

    public required long ImageId;
    public required string UserId;
}