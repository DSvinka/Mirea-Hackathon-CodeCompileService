namespace Shared.Utils.Redis.Enums;

public enum EErrorReasonType
{
    UnknownException,
    
    AlreadyExist,
    ToManyExist,
    
    IncorrectCredentials,
    IncorrectRequest,
    
    Timeout,
    
    NotAdministrator,
    NotAuthorized,
    NotFound,
}