using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class VoucherPool
{
    public Guid VoucherPoolId { get; set; }

    public Guid VoucherDefId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual VoucherDefinition VoucherDef { get; set; } = null!;

    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
