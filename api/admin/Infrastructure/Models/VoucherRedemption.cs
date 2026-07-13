using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class VoucherRedemption
{
    public Guid VoucherRedemptionId { get; set; }

    public Guid CustomerVoucherId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VoucherDefId { get; set; }

    public Guid? VoucherPoolId { get; set; }

    public Guid? CampaignId { get; set; }

    public Guid? CampaignSessionId { get; set; }

    public Guid? ActionId { get; set; }

    public string? SourceEventId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public DateTime RedeemedAt { get; set; }

    public virtual Action? Action { get; set; }

    public virtual Campaign? Campaign { get; set; }

    public virtual CampaignSession? CampaignSession { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual CustomerVoucher CustomerVoucher { get; set; } = null!;

    public virtual VoucherDefinition VoucherDef { get; set; } = null!;

    public virtual VoucherPool? VoucherPool { get; set; }
}
