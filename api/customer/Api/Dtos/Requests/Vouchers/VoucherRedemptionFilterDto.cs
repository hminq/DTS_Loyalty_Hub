namespace Api.Dtos.Requests.Vouchers;

public sealed class VoucherRedemptionFilterDto
{
    public string? Name { get; set; }
    public string? VoucherDefRewardType { get; set; }
    public DateTime? RedeemAtFrom { get; set; }
    public DateTime? RedeemAtTo { get; set; }
    public string? CampaignName { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
