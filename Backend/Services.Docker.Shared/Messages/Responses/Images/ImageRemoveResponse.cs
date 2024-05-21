namespace Services.Docker.Shared.Messages.Responses.Images;

public class ImageRemoveResponse
{
    public required string ConnectionId;
    
    public required long ImageId;
}