namespace Services.Docker.Shared.Messages.Responses.Images;

public class ImageResponse
{
    public required string ConnectionId;
    
    public required string DisplayName;
    public required string Description;

    public required string DockerImage;
    public required string DockerImageTag;

    public required string CodeFileExtension;
    public required string CodeEditorLang;
    
    public string? CodeInitCommand;
    public required string CodeStartCommand;

    public required int MaxMemory;
    public required int MaxCpuShares;

    /// <summary>
    /// In Megabytes
    /// </summary>
    public required int MaxStorage;

    public required int MaxCountByUser;
    public required int MaxCountByServer;
}