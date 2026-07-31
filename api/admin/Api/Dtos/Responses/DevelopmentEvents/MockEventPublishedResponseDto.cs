using Messaging.Contracts.Events;

namespace Api.Dtos.Responses.DevelopmentEvents;

public sealed class MockEventPublishedResponseDto
{
    public Guid EventId { get; set; }

    public string EventType { get; set; } = null!;

    public int EventVersion { get; set; }

    public DateTime OccurredAt { get; set; }

    public string RoutingKey { get; set; } = null!;

    public CustomerMissionCompletedPayload Payload { get; set; } = null!;
}
