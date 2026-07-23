namespace Api.Dtos.Responses.CustomerVouchers;

public sealed class CustomerRedeemResponseDto
{
    public Guid VoucherRedemptionId { get; init; }

    public CustomerInfoResponseDto CusInfo { get; init; } = new();

    public Guid CusVoucherId { get; init; }

    public Guid VoucherDefId { get; init; }

    public string VoucherDefName { get; init; } = string.Empty;

    public string? VoucherDefDescription { get; init; }

    public Guid? CampaignId { get; init; }

    public string? CampaignName { get; init; }

    public DateTime ValidFrom { get; init; }

    public DateTime ValidTo { get; init; }

    public DateTime RedeemAt { get; init; }
}
