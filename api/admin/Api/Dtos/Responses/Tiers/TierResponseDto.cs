namespace Api.Dtos.Responses.Tiers;

public sealed class TierResponseDto
{
    public Guid TierConfigId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PointsRequired { get; set; }

    public int CycleMonth { get; set; }

    public int Priority { get; set; }
}
