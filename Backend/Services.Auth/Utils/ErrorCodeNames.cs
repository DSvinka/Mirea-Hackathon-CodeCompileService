namespace Services.Auth.Utils;

public static class ErrorCodeNames
{
    public const string AccessTokenIsIncorrect = "accessTokenIsIncorrect";
    public const string PasswordIsIncorrect = "passwordIsIncorrect";
    
    public const string UserNotFound = "userNotFound";
    
    public const string EmailIsAlreadyUse = "emailIsAlreadyUse";
    public const string PhoneIsAlreadyUse = "phoneIsAlreadyUse";

    public const string RefreshTokenIsExpire = "refreshTokenIsExpire";
}