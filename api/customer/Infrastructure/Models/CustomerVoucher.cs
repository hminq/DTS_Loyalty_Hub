using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class CustomerVoucher
{
    public Guid CustomerVoucherId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VoucherDefId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public Guid? VoucherPoolId { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public int RemainingCount { get; set; }

    public DateTime RedeemedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual VoucherDefinition VoucherDef { get; set; } = null!;

    public virtual VoucherPool? VoucherPool { get; set; }

    public virtual VoucherRedemption? VoucherRedemption { get; set; }
}
