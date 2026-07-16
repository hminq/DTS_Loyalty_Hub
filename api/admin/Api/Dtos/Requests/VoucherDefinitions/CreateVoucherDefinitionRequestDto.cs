namespace Api.Dtos.Requests.VoucherDefinitions;

public sealed class CreateVoucherDefinitionRequestDto
{
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? BannerImageUrl { get; set; }

    public string RewardType { get; set; } = string.Empty;

    public decimal? RewardValue { get; set; }

    public string ValidityType { get; set; } = string.Empty;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public int? DurationDay { get; set; }

    public string GenerationType { get; set; } = string.Empty;

    public string PublishType { get; set; } = string.Empty;

    public int TotalStock { get; set; }
}
