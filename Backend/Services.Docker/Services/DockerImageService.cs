using Docker.DotNet;
using Docker.DotNet.Models;

namespace Services.Docker.Services;

public class DockerImageService
{
    private readonly DockerClient _client;

    public DockerImageService(DockerClient client)
    {
        _client = client;
    }
    
    public async Task<bool> TryImageAddAsync(string imageName, string imageTag, CancellationToken cancellationToken = default)
    {
        var parameters = new ImagesCreateParameters
        {
            FromImage = imageName,
            Tag = imageTag
        };

        var exist = await ImageExistAsync(imageName, imageTag, cancellationToken);
        if (exist)
            return false;
        
        var progress = new Progress<JSONMessage>();
        await _client.Images.CreateImageAsync(parameters, null, progress, cancellationToken);
        
        return true;
    }
    
    public async Task<bool> TryImageRemoveAsync(string imageName, string imageTag, CancellationToken cancellationToken = default)
    {
        var exist = await ImageExistAsync(imageName, imageTag, cancellationToken);
        if (!exist)
            return false;
        
        var parameters = new ImageDeleteParameters
        {
            Force = false,
            NoPrune = false
        };
        
        await _client.Images.DeleteImageAsync($"{imageName}:{imageTag}", parameters, cancellationToken);
        return true;
    }
    
    public async Task<IList<ImagesListResponse>?> ImageListAsync(CancellationToken cancellationToken = default)
    {
        var parameters = new ImagesListParameters
        {
            All = false,
            Filters = null,
            Digests = false
        };
        
        var images = await _client.Images.ListImagesAsync(parameters, cancellationToken);
        return images;
    }
    
    public async Task<bool> ImageExistAsync(string imageName, string imageTag, CancellationToken cancellationToken = default)
    {
        var images = await _client.Images.ListImagesAsync(new ImagesListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                {"reference", new Dictionary<string, bool>() {{$"{imageName}:{imageTag}", true}}}
            }
        }, cancellationToken);
        
        if (images == null || images.Count == 0)
            return false;

        return true;
    }
}