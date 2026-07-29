namespace Consumer.Core.Entities.Campaigns;

public sealed record ValidatedCustomerAccountRegisteredEvent(
    Guid EventId,
    string EventType,
    string RoutingKey,
    DateTime OccurredAt,
    Guid UserId,
    Guid CustomerId,
    string Source,
    Guid? ReferrerCustomerId,
    string NormalizedPayload,
    string PayloadHash)
    : IValidatedCampaignEvent
{
    public Guid PrimaryCustomerId => CustomerId;
}
