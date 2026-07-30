using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class EventType
{
    public Guid EventTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string RoutingKey { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<EventTypeVersion> EventTypeVersions { get; set; } =
        new List<EventTypeVersion>();
}
