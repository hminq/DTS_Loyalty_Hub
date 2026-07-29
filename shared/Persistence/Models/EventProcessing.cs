using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class EventProcessing
{
    public Guid EventId { get; set; }

    public string EventType { get; set; } = null!;

    public string RoutingKey { get; set; } = null!;

    public Guid? EventTypeVersionId { get; set; }

    public int? EventVersion { get; set; }

    public DateTime OccurredAt { get; set; }

    public string Payload { get; set; } = null!;

    public string PayloadHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public virtual ICollection<EventCampaignProcessing> EventCampaignProcessings { get; set; } =
        new List<EventCampaignProcessing>();

    public virtual EventTypeVersion? EventTypeVersion { get; set; }
}
