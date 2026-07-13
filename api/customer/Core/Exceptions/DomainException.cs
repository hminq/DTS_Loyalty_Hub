namespace Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string errorCode, string message, DomainErrorType errorType)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
    }

    public string ErrorCode { get; }

    public DomainErrorType ErrorType { get; }
}
