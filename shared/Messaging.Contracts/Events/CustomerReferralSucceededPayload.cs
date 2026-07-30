namespace Messaging.Contracts.Events;

public sealed record CustomerReferralSucceededPayload(
    Guid ReferrerCustomerId,
    Guid ReferredCustomerId,
    string ReferredUsername);
