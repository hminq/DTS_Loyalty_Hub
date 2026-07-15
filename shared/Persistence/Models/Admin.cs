using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class Admin
{
    public Guid AdminId { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
