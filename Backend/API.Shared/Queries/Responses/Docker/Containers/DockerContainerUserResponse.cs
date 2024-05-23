using Services.Docker.Shared.Enums;

namespace API.Shared.Queries.Responses.Docker.Containers;

public class DockerContainerUserResponse
{
    public required long Id;
    
    public required long UserId;
    public required string ContainerId;
    
    public required EDockerStatus Status;
    public string? Logs;
    
    public required string ProgramCode;

    public required int UsageMemory;
    public required int UsageCpu;
    public required int UsageStorage;
}