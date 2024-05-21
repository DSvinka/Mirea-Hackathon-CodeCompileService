namespace Services.Docker.Shared.Messages.Responses.Images;

public class ImageListResponse
{
    public required string ConnectionId;
    
    public required List<ImageResponse> Images;
}