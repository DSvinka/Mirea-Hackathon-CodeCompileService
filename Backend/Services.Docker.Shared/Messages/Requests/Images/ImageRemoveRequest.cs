namespace Services.Docker.Shared.Messages.Requests.Images;

public class ImageRemoveRequest
{
    public required string ConnectionId;
    
    public required long ImageId;
}