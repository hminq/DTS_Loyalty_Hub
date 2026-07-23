namespace Api.Dtos.Requests.CustomerVouchers;

public sealed class GetCustomerRedeemsRequestDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? VoucherKeyword { get; init; }

    public string? RewardType { get; init; }

    public DateTime? RedeemAtFrom { get; init; }

    public DateTime? RedeemAtTo { get; init; }

    public string? CampaignName { get; init; }

    public string? UserKeyword { get; init; }
}
