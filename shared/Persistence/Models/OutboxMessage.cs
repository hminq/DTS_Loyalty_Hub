using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class OutboxMessage
{
    public Guid EventId { get; set; }

    public string EventType { get; set; } = null!;

    public string RoutingKey { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public string? LastErrorCode { get; set; }

    public string? LastError { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}
