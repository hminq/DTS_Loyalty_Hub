namespace Api.Dtos.Responses.Tiers;

public sealed class TierListItemResponseDto
{
    public Guid TierConfigId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PointsRequired { get; set; }

    public int Priority { get; set; }
}

public sealed class TierDetailResponseDto
{
    public Guid TierConfigId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PointsRequired { get; set; }

    public int CycleMonth { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class TierResponseDto
{
    public Guid TierConfigId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PointsRequired { get; set; }

    public int CycleMonth { get; set; }

    public int Priority { get; set; }
}
