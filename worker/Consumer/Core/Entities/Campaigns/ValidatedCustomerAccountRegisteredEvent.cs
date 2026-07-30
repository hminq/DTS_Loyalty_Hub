namespace Consumer.Core.Entities.Campaigns;

public sealed record ValidatedCustomerAccountRegisteredEvent(
    Guid EventId,
    string EventType,
    Guid EventTypeVersionId,
    int EventVersion,
    string RoutingKey,
    DateTime OccurredAt,
    Guid UserId,
    Guid CustomerId,
    string Source,
    Guid? ReferrerCustomerId,
    string NormalizedPayload,
    string PayloadHash)
    : IValidatedCampaignEvent;
