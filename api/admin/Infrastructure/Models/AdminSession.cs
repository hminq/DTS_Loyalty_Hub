using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class AdminSession
{
    public Guid AdminSessionId { get; set; }

    public Guid AdminId { get; set; }

    public Guid UserId { get; set; }

    public Guid AccessTokenJti { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Admin Admin { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
