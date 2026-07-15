using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class CampaignVoucherOption
{
    public Guid CampaignVoucherOptionId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid VoucherDefinitionId { get; set; }

    public decimal PointCost { get; set; }

    public int? LimitPerCustomer { get; set; }

    public int DisplayOrder { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual VoucherDefinition VoucherDefinition { get; set; } = null!;
}
