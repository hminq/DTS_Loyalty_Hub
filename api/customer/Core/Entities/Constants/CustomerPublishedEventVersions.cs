using Messaging.Contracts.Events;

namespace Core.Entities.Constants;

public static class CustomerPublishedEventVersions
{
    public const int CustomerAccountRegisteredV1 = 1;
    public const int CustomerReferralSucceededV1 = 1;

    public static readonly (string EventType, int EventVersion) CustomerAccountRegistered =
        (EventTypeCodes.CustomerAccountRegistered, CustomerAccountRegisteredV1);

    public static readonly (string EventType, int EventVersion) CustomerReferralSucceeded =
        (EventTypeCodes.CustomerReferralSucceeded, CustomerReferralSucceededV1);
}
