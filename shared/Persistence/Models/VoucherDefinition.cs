using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class VoucherDefinition
{
    public Guid VoucherDefinitionId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? BannerImageUrl { get; set; }

    public string RewardType { get; set; } = null!;

    public decimal? RewardValue { get; set; }

    public string ValidityType { get; set; } = null!;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public int? DurationDay { get; set; }

    public string GenerationType { get; set; } = null!;

    public string PublishType { get; set; } = null!;

    public int TotalStock { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CampaignVoucherOption> CampaignVoucherOptions { get; set; } = new List<CampaignVoucherOption>();

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<VoucherPool> VoucherPools { get; set; } = new List<VoucherPool>();

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
