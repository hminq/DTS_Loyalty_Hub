namespace Api.Dtos.Responses.Vouchers;

public sealed class VoucherRedemptionResponseDto
{
    public Guid VoucherRedemptionId { get; set; }
    public Guid CusVoucherId { get; set; }
    public Guid VoucherDefId { get; set; }
    public string VoucherDefName { get; set; } = string.Empty;
    public string? VoucherDefDescription { get; set; }
    public Guid? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime RedeemAt { get; set; }
}
