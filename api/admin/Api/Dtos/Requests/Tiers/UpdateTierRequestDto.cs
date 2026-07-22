namespace Api.Dtos.Requests.Tiers;

public sealed class UpdateTierRequestDto
{
    public string Name { get; set; } = string.Empty;

    public decimal PointsRequired { get; set; }

    public int CycleMonth { get; set; }

    public int Priority { get; set; }
}