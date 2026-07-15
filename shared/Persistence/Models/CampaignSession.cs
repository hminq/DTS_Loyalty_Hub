using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class CampaignSession
{
    public Guid CampaignSessionId { get; set; }

    public Guid CampaignId { get; set; }

    public DateTime SessionStart { get; set; }

    public DateTime SessionEnd { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual ICollection<ActionUsage> ActionUsages { get; set; } = new List<ActionUsage>();

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual ICollection<CampaignUsage> CampaignUsages { get; set; } = new List<CampaignUsage>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
