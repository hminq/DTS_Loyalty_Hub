namespace Consumer.Core.Entities.Campaigns;

public interface IValidatedCampaignEvent
{
    Guid EventId { get; }

    string EventType { get; }

    string RoutingKey { get; }

    DateTime OccurredAt { get; }

    Guid PrimaryCustomerId { get; }

    string NormalizedPayload { get; }

    string PayloadHash { get; }
}
