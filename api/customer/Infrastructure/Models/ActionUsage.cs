using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ActionUsage
{
    public Guid ActionUsageId { get; set; }

    public Guid ActionId { get; set; }

    public Guid CampaignSessionId { get; set; }

    public int UsedCount { get; set; }

    public decimal UsedAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Action Action { get; set; } = null!;

    public virtual CampaignSession CampaignSession { get; set; } = null!;
}
