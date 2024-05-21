namespace API.Shared.Queries.Responses.Docker.Images;

public class ImageAdminResponse: ImageUserResponse
{
    public required string DockerImage;
    public required string DockerImageTag;
    
    public string? CodeInitCommand;
    public required string CodeStartCommand;

    public required int MaxMemory;
    public required int MaxCpuShares;

    /// <summary>
    /// In Megabytes
    /// </summary>
    public required int MaxStorage;

    public required int MaxCountByServer;
}