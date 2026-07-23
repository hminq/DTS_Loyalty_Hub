namespace Api.Dtos.Responses.CustomerVouchers;

public sealed class CustomerVoucherResponseDto
{
    public Guid CusVoucherId { get; init; }

    public CustomerInfoResponseDto CusInfo { get; init; } = new();

    public string VoucherDefName { get; init; } = string.Empty;

    public string VoucherDefRewardType { get; init; } = string.Empty;

    public bool IsExpired { get; init; }
}
