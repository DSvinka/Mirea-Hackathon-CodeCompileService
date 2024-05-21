namespace Services.Docker.Utils;

public static class ErrorCodeNames
{
    // AUTH
    public const string AccessTokenIsIncorrect = "accessTokenIsIncorrect";
    public const string PasswordIsIncorrect = "passwordIsIncorrect";
    
    public const string UserNotFound = "userNotFound";
    
    public const string EmailIsAlreadyUse = "emailIsAlreadyUse";
    public const string PhoneIsAlreadyUse = "phoneIsAlreadyUse";

    public const string RefreshTokenIsExpire = "refreshTokenIsExpire";
    
    
    // DOCKER
    public const string DockerContainerNotFound = "containerNotFound";
    public const string DockerImageNotFound = "imageNotFound";
    
    public const string DockerImageAlreadyExist = "imageAlreadyExist";
    public const string DockerContainerAlreadyExist = "containerAlreadyExist";
    
    public const string DockerContainersUserLimitReached = "containerUserLimitReached";
    public const string DockerContainersServerLimitReached = "containerServerLimitReached";
}