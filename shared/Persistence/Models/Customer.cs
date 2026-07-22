using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public Guid UserId { get; set; }

    public Guid? TierId { get; set; }

    public decimal CurrentTierPoint { get; set; }

    public decimal NextTierPoint { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartTier { get; set; }

    public DateTime? ExpiredTier { get; set; }

    public Guid? NextTierId { get; set; }

    public virtual ICollection<CampaignUsage> CampaignUsages { get; set; } = new List<CampaignUsage>();

    public virtual CustomerPoint? CustomerPoint { get; set; }

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual TiersConfig? Tier { get; set; }

    public virtual TiersConfig? NextTier { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();

    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
