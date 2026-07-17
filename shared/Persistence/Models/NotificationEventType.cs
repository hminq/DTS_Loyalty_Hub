using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class NotificationEventType
{
    public Guid NotificationEventTypeId { get; set; }

    public string EventTypeCode { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Description { get; set; }

    public string AvailableVariables { get; set; } = "[]";

    public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();
}
