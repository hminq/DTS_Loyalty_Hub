namespace Core.Exceptions;

public sealed class CampaignProcessingException : Exception
{
    public CampaignProcessingException(string errorCode, bool retriable)
        : base(errorCode)
    {
        ErrorCode = errorCode;
        Retriable = retriable;
    }

    public string ErrorCode { get; }

    public bool Retriable { get; }
}
