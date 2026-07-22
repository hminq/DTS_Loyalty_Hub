namespace Api.Dtos.Responses.CustomerUsers;

public sealed class CustomerVoucherResponseDto
{
    public Guid CustomerVoucherId { get; set; }

    public Guid VoucherDefinitionId { get; set; }

    public string VoucherDefinitionName { get; set; } = null!;

    public Guid? VoucherPoolId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public int RemainingCount { get; set; }

    public DateTime ReceivedAt { get; set; }
}

public sealed class CustomerVoucherRedemptionResponseDto
{
    public Guid VoucherRedemptionId { get; set; }

    public Guid CustomerVoucherId { get; set; }

    public Guid VoucherDefinitionId { get; set; }

    public string VoucherDefinitionName { get; set; } = null!;

    public Guid? VoucherPoolId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public Guid? CampaignId { get; set; }

    public string? CampaignName { get; set; }

    public Guid? CampaignSessionId { get; set; }

    public Guid? ActionId { get; set; }

    public string? ActionType { get; set; }

    public string? SourceEventId { get; set; }

    public DateTime RedeemedAt { get; set; }
}

public sealed class CustomerPointTransactionResponseDto
{
    public Guid PointTransactionId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public Guid? CampaignId { get; set; }

    public string? CampaignName { get; set; }

    public Guid? CampaignSessionId { get; set; }

    public Guid? ActionId { get; set; }

    public string? ActionType { get; set; }

    public string? SourceEventId { get; set; }

    public DateTime CreatedAt { get; set; }
}
