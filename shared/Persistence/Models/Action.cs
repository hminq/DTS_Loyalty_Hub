using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class Action
{
    public Guid ActionId { get; set; }

    public string ReferenceType { get; set; } = null!;

    public Guid ReferenceId { get; set; }

    public string ActionType { get; set; } = null!;

    public string ActionConfig { get; set; } = null!;

    public int ExecuteOrder { get; set; }

    public int? TotalCount { get; set; }

    public int? SessionCount { get; set; }

    public int UsedCount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? SessionAmount { get; set; }

    public decimal UsedAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ActionUsage> ActionUsages { get; set; } = new List<ActionUsage>();

    public virtual ICollection<CampaignUsage> CampaignUsages { get; set; } = new List<CampaignUsage>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
