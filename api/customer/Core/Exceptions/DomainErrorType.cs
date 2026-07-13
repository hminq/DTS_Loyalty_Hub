namespace Core.Exceptions;

public enum DomainErrorType
{
    Validation, 
    Conflict, 
    NotFound, 
    Forbidden,
    Unauthorized
}
