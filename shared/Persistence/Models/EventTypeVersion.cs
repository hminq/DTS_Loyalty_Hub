using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class EventTypeVersion
{
    public Guid EventTypeVersionId { get; set; }

    public Guid EventTypeId { get; set; }

    public int Version { get; set; }

    public string PayloadSchema { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual EventType EventType { get; set; } = null!;

    public virtual ICollection<EventProcessing> EventProcessings { get; set; } =
        new List<EventProcessing>();

    public virtual ICollection<OutboxMessage> OutboxMessages { get; set; } =
        new List<OutboxMessage>();
}
