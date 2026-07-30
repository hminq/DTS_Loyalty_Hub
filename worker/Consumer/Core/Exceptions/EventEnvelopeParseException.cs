using Consumer.Core.Entities.Constants;

namespace Consumer.Core.Exceptions;

public sealed class EventEnvelopeParseException : Exception
{
    public EventEnvelopeParseException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public EventEnvelopeParseException(string errorCode)
        : base(errorCode)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
