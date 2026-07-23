namespace Api.Dtos.Responses.CustomerUsers;

public sealed class CustomerUserListItemResponseDto
{
    public Guid CustomerId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public Guid? TierId { get; set; }

    public string? TierName { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

public sealed class CustomerUserTierResponseDto
{
    public Guid TierId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PointsRequired { get; set; }

    public int CycleMonth { get; set; }

    public int Priority { get; set; }
}

public sealed class CustomerUserResponseDto
{
    public Guid CustomerId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public decimal CurrentTierPoint { get; set; }

    public decimal NextTierPoint { get; set; }

    public CustomerUserTierResponseDto? Tier { get; set; }
}

public sealed class CustomerUserPointsResponseDto
{
    public Guid CustomerId { get; set; }

    public decimal CurrentTierPoint { get; set; }

    public decimal NextTierPoint { get; set; }

    public CustomerUserTierResponseDto? Tier { get; set; }

    public decimal ActivePoint { get; set; }

    public decimal LockedPoint { get; set; }

    public decimal LifetimePoint { get; set; }

    public decimal SpentPoint { get; set; }

    public decimal ExpiredPoint { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
