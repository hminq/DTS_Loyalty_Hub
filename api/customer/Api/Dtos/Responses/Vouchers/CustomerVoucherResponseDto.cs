namespace Api.Dtos.Responses.Vouchers;

public sealed class CustomerVoucherResponseDto
{
    public Guid CusVoucherId { get; set; }
    public string VoucherDefName { get; set; } = string.Empty;
    public string VoucherDefRewardType { get; set; } = string.Empty;
    public bool IsExpired { get; set; }
}

public sealed class CustomerVoucherDetailResponseDto
{
    public Guid CusVoucherId { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
    public Guid VoucherDefId { get; set; }
    public string VoucherDefName { get; set; } = string.Empty;
    public string? VoucherDefDescription { get; set; }
    public string VoucherDefRewardType { get; set; } = string.Empty;
    public string? VoucherDefBannerImgUrl { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime RedeemAt { get; set; }
}
