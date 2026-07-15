namespace Api.Dtos.Responses.Users;

public class PointTransactionResponseDto
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SourceEventId { get; set; }
}
