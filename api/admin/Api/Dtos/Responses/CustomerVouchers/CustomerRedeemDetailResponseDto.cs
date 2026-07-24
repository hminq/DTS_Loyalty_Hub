namespace Api.Dtos.Responses.CustomerVouchers;

public sealed class CustomerRedeemDetailResponseDto
{
    public Guid VoucherRedemptionId { get; init; }

    public DateTime RedeemedAt { get; init; }

    public CustomerRedeemCustomerResponseDto Customer { get; init; } = new();

    public CustomerRedeemVoucherResponseDto Voucher { get; init; } = new();

    public CustomerRedeemIssuanceSourceResponseDto IssuanceSource { get; init; } = new();
}

public sealed class CustomerRedeemCustomerResponseDto
{
    public Guid CustomerId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }
}

public sealed class CustomerRedeemVoucherResponseDto
{
    public Guid CustomerVoucherId { get; init; }

    public Guid VoucherDefinitionId { get; init; }

    public Guid? VoucherPoolId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? BannerImageUrl { get; init; }

    public string VoucherCode { get; init; } = string.Empty;

    public string RewardType { get; init; } = string.Empty;

    public decimal? RewardValue { get; init; }

    public string GenerationType { get; init; } = string.Empty;

    public DateTime ValidFrom { get; init; }

    public DateTime ValidTo { get; init; }
}

public sealed class CustomerRedeemIssuanceSourceResponseDto
{
    public Guid? CampaignId { get; init; }

    public string? CampaignName { get; init; }

    public string? CampaignEventType { get; init; }

    public Guid? CampaignSessionId { get; init; }

    public DateTime? SessionStart { get; init; }

    public DateTime? SessionEnd { get; init; }

    public string? SessionStatus { get; init; }

    public Guid? ActionId { get; init; }

    public string? ActionType { get; init; }
}
