namespace Consumer.Core.Entities.Campaigns;

public interface IValidatedCampaignEvent
{
    Guid EventId { get; }

    string EventType { get; }

    Guid EventTypeVersionId { get; }

    int EventVersion { get; }

    string RoutingKey { get; }

    DateTime OccurredAt { get; }

    string NormalizedPayload { get; }

    string PayloadHash { get; }
}
