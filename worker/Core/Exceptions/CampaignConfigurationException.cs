namespace Core.Exceptions;

public sealed class CampaignConfigurationException : Exception
{
    public CampaignConfigurationException(string errorCode)
        : base(errorCode)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
