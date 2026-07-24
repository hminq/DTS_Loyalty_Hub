using System;

namespace Persistence.Models;

public partial class VoucherPoolImportRow
{
    public Guid JobId { get; set; }

    public int RowNumber { get; set; }

    public Guid VoucherPoolId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual VoucherPoolProvisioningJob Job { get; set; } = null!;
}
