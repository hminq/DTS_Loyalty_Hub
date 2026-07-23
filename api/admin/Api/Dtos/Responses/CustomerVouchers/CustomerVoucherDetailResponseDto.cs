namespace Api.Dtos.Responses.CustomerVouchers;

public sealed class CustomerVoucherDetailResponseDto
{
    public Guid CusVoucherId { get; init; }

    public CustomerInfoResponseDto CusInfo { get; init; } = new();

    public Guid VoucherDefId { get; init; }

    public string VoucherDefName { get; init; } = string.Empty;

    public string? VoucherDefDescription { get; init; }

    public string VoucherDefRewardType { get; init; } = string.Empty;

    public string? VoucherDefBannerImgUrl { get; init; }

    public string VoucherDefGenerationType { get; init; } = string.Empty;

    public DateTime ValidFrom { get; init; }

    public DateTime ValidTo { get; init; }

    public DateTime RedeemAt { get; init; }
}
