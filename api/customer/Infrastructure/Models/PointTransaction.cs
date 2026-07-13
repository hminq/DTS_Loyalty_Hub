using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class PointTransaction
{
    public Guid PointTransactionId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? CampaignId { get; set; }

    public Guid? CampaignSessionId { get; set; }

    public Guid? ActionId { get; set; }

    public string? SourceEventId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Action? Action { get; set; }

    public virtual Campaign? Campaign { get; set; }

    public virtual CampaignSession? CampaignSession { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
