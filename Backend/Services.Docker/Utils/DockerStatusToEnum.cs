using Services.Docker.Shared.Enums;

namespace Services.Docker.Utils;

public static class DockerStatusToEnum
{
    public static EDockerStatus Convert(string status)
    {
        return status switch
        {
            "dead" => EDockerStatus.Dead,
            "exist" => EDockerStatus.Exist,
            "created" => EDockerStatus.Created,
            "running" => EDockerStatus.Running,
            "restarting" => EDockerStatus.Restarting,
            "paused" => EDockerStatus.Paused,
            "stopped" => EDockerStatus.Stopped,
            _ => EDockerStatus.Unknown
        };
    }
}