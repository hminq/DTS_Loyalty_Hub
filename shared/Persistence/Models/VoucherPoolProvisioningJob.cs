using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class VoucherPoolProvisioningJob
{
    public Guid JobId { get; set; }

    public Guid VoucherDefId { get; set; }

    public string JobType { get; set; } = null!;

    public string? ImportFileKey { get; set; }

    public int ExpectedCount { get; set; }

    public int ProcessedCount { get; set; }

    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorDetails { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual VoucherDefinition VoucherDef { get; set; } = null!;

    public virtual ICollection<VoucherPoolImportRow> VoucherPoolImportRows { get; set; } = new List<VoucherPoolImportRow>();
}
