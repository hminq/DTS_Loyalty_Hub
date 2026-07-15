using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class Campaign
{
    public Guid CampaignId { get; set; }

    public string CampaignName { get; set; } = null!;

    public string? Description { get; set; }

    public string? BannerImageUrl { get; set; }

    public string EventType { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Condition { get; set; } = null!;

    public decimal? MinAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public string? ScheduleCron { get; set; }

    public int? DurationHour { get; set; }

    public int? UserLimitTotal { get; set; }

    public int? UserLimitSession { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CampaignSession> CampaignSessions { get; set; } = new List<CampaignSession>();

    public virtual ICollection<CampaignUsage> CampaignUsages { get; set; } = new List<CampaignUsage>();

    public virtual ICollection<CampaignVoucherOption> CampaignVoucherOptions { get; set; } = new List<CampaignVoucherOption>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
