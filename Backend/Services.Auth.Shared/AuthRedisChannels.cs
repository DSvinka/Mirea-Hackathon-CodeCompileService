namespace Services.Auth.Shared;

public static class AuthRedisChannels
{
    public const string LoginChannelRequest = "auth-login-request";
    public const string LoginChannelResponse = "auth-login-response";
    
    public const string ProfileChannelRequest = "auth-profile-request";
    public const string ProfileChannelResponse = "auth-profile-response";
    
    public const string RefreshChannelRequest = "auth-refresh-request";
    public const string RefreshChannelResponse = "auth-refresh-response";

    public const string RegisterChannelRequest = "auth-register-request";
    public const string RegisterChannelResponse = "auth-register-response";
    
    public const string VerifyChannelRequest = "auth-verify-request";
    public const string VerifyChannelResponse = "auth-verify-response";
}