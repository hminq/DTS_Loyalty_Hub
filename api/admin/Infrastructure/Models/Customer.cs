using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public Guid UserId { get; set; }

    public Guid? TierId { get; set; }

    public decimal CurrentTierPoint { get; set; }

    public decimal NextTierPoint { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CampaignUsage> CampaignUsages { get; set; } = new List<CampaignUsage>();

    public virtual CustomerPoint? CustomerPoint { get; set; }

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual TiersConfig? Tier { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
