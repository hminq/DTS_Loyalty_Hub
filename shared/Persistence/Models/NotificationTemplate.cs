using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class NotificationTemplate
{
    public Guid TemplateId { get; set; }

    public Guid NotificationEventTypeId { get; set; }

    public string Channel { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string TitleTemplate { get; set; } = null!;

    public string BodyTemplate { get; set; } = null!;

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual NotificationEventType NotificationEventType { get; set; } = null!;

    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
