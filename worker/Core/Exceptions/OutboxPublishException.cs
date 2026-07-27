namespace Core.Exceptions;

public sealed class OutboxPublishException : Exception
{
    public OutboxPublishException(string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
