namespace Consumer.Core.Exceptions;

public sealed class CampaignProcessingException : Exception
{
    public CampaignProcessingException(
        string errorCode,
        bool retriable,
        Exception? innerException = null)
        : base(errorCode, innerException)
    {
        ErrorCode = errorCode;
        Retriable = retriable;
    }

    public string ErrorCode { get; }

    public bool Retriable { get; }
}
