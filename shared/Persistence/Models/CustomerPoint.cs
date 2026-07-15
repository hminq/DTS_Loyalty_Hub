using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class CustomerPoint
{
    public Guid CustomerPointId { get; set; }

    public Guid CustomerId { get; set; }

    public decimal ActivePoint { get; set; }

    public decimal LockedPoint { get; set; }

    public decimal LifetimePoint { get; set; }

    public decimal SpentPoint { get; set; }

    public decimal ExpiredPoint { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
