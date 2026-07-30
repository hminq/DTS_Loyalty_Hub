namespace Campaign.Contracts.Constants;

/// <summary>
/// UI/configuration choices for conditions on CUSTOMER_ACCOUNT_REGISTERED campaigns.
/// These codes are not event payload source values.
/// </summary>
public static class CustomerRegistrationConditionOptionCodes
{
    public const string AllRegistrations = "ALL_REGISTRATIONS";
    public const string NormalRegistration = "NORMAL_REGISTRATION";
    public const string ReferralRegistration = "REFERRAL_REGISTRATION";
}
