using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class CampaignUsage
{
    public Guid CampaignUsageId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid? CampaignSessionId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid ActionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Action Action { get; set; } = null!;

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual CampaignSession? CampaignSession { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
