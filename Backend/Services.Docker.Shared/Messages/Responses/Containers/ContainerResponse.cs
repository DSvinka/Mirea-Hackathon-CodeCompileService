﻿using Services.Docker.Shared.Enums;

namespace Services.Docker.Shared.Messages.Responses.Containers;

public class ContainerResponse
{
    public required string ConnectionId;

    public required long UserId;
    public required string ContainerId;
    
    public required EDockerStatus Status;
    public string? Logs;

    public required int Memory;
    public required int CpuShares;
    public required int Storage;
}