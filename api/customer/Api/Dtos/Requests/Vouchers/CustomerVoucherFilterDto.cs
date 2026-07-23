namespace Api.Dtos.Requests.Vouchers;

public sealed class CustomerVoucherFilterDto
{
    public string? VoucherKeyword { get; set; }
    public string? RewardType { get; set; }
    public DateTime? RedeemAtFrom { get; set; }
    public DateTime? RedeemAtTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
