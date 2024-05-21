using Services.Docker.Shared.Enums;

namespace API.Shared.Queries.Responses.Docker.Containers;

public class ContainerUserResponse
{
    public required long UserId;
    public required string ContainerId;
    
    public required EDockerStatus Status;
    public string? Logs;

    public required int Memory;
    public required int CpuShares;
    public required int Storage;
}