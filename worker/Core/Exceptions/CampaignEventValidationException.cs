namespace Core.Exceptions;

public sealed class CampaignEventValidationException : Exception
{
    public CampaignEventValidationException(string errorCode)
        : base(errorCode)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
