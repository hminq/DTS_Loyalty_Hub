namespace Api.Dtos.Responses.VoucherDefinitions;

public sealed class VoucherDefinitionListItemResponseDto
{
    public Guid VoucherDefinitionId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = null!;

    public string RewardType { get; set; } = null!;

    public decimal? RewardValue { get; set; }

    public string PublishType { get; set; } = null!;

    public int TotalStock { get; set; }

    public int RemainingStock { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
