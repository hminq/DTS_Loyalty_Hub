using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class TiersConfig
{
    public Guid TierConfigId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PointsRequired { get; set; }

    public int CycleMonth { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Customer> NextTierCustomers { get; set; } = new List<Customer>();
}
