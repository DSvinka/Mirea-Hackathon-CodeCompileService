namespace API.Utils;

public static class ErrorCodeNames
{
    public const string AccessTokenIsIncorrect = "accessTokenIsIncorrect";
    public const string RefreshTokenIsIncorrect = "refreshTokenIsIncorrect";
    
    public const string LoginOrPasswordIsIncorrect = "loginOrPsswordIsIncorrect";
    
    public const string UserNotFound = "userNotFound";
    
    public const string EmailIsAlreadyUse = "emailIsAlreadyUse";
    public const string PhoneIsAlreadyUse = "phoneIsAlreadyUse";

    public const string RefreshTokenIsExpire = "refreshTokenIsExpire";

    public const string ProgramCodeIsToLong = "programCodeIsToLong";
}