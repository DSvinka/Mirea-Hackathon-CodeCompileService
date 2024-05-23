using Services.Docker.Shared.Enums;

namespace Services.Docker.Shared.Messages.Responses.Containers;

public class ContainerResponse
{
    public required long Id;
    
    public string? ConnectionId;

    public required long UserId;
    public required string ContainerId;
    
    public required EDockerStatus Status;
    public string? Logs;
    
    public required string ProgramCode;
    public required string ProgramCodeFolder;
    
    public required int UsageMemory;
    public required int UsageCpu;
    public required int UsageStorage;
}