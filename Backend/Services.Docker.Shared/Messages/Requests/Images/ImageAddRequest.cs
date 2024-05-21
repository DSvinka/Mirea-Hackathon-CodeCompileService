namespace Services.Docker.Shared.Messages.Requests.Images;

public class ImageAddRequest
{
    public required string ConnectionId;
    
    public required string Image;
    public required string ImageTag;

    public required string DisplayName;
    public required string Description;
    
    public required string CodeFileExtension;
    public required string CodeEditorLang;
    
    public required string? CodeInitCommand;
    public required string CodeStartCommand;

    /// <summary>
    /// In Megabytes
    /// </summary>
    public required int Memory;
    
    /// <summary>
    /// For example 512 is 1/2 of CPU Core
    /// </summary>
    public required int CpuShares;

    /// <summary>
    /// In Megabytes
    /// </summary>
    public required int MaxStorage;


    public required int MaxCountByUser;
    public required int MaxCountByServer;
}