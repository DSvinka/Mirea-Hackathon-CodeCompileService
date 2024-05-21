namespace Services.Docker.Shared.Enums;

public enum EDockerStatus
{
    Dead,
    Exist,
    Paused,
    Running,
    Stopped,
    Created,
    Restarting,
    Unknown
}