using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class NotificationLog
{
    public Guid LogId { get; set; }

    public Guid? TemplateId { get; set; }

    public string EventTypeCode { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public string? RenderedTitle { get; set; }

    public string RenderedBody { get; set; } = null!;

    public string? VariablesSnapshot { get; set; }

    public string? SourceRefType { get; set; }

    public Guid? SourceRefId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual NotificationTemplate? Template { get; set; }
}
