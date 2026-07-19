namespace Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(
        string errorCode,
        DomainErrorType errorType,
        params object[] messageArguments)
        : base(errorCode)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
        MessageArguments = messageArguments;
    }

    public string ErrorCode { get; }

    public DomainErrorType ErrorType { get; }

    public IReadOnlyList<object> MessageArguments { get; }
}
