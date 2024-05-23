namespace Services.Docker.Shared;

public static class DockerRedisChannels
{
    // Containers
    public const string ContainerCreateAndRunChannelRequest = "docker-container-createandrun-request";
    public const string ContainerCreateAndRunChannelResponse = "docker-container-createandrun-response";
    
    public const string ContainerDeleteAndStopChannelRequest = "docker-container-deleteandstop-request";
    public const string ContainerDeleteAndStopChannelResponse = "docker-container-deleteandstop-response";
    
    public const string ContainerListChannelRequest = "docker-container-list-request";
    public const string ContainerListChannelResponse = "docker-container-list-response";

    public const string ContainerStatusChannelRequest = "docker-container-status-request";
    public const string ContainerStatusChannelResponse = "docker-container-status-response";
    
    public const string ContainerErrorChannelResponse = "docker-image-error-response";
    public const string ContainerMonitoringResponse = "docker-containers-monitoring-response";
    
    
    // Images
    public const string ImageAddChannelRequest = "docker-image-add-request";
    public const string ImageAddChannelResponse = "docker-image-add-response";
    
    public const string ImageListChannelRequest = "docker-image-list-request";
    public const string ImageListChannelResponse = "docker-image-list-response";
    
    public const string ImageRemoveChannelRequest = "docker-image-remove-request";
    public const string ImageRemoveChannelResponse = "docker-image-remove-response";

    public const string ImageErrorChannelResponse = "docker-image-error-response";
}