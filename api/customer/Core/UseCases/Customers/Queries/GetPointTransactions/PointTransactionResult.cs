namespace Core.UseCases.Customers.Queries.GetPointTransactions;

public record PointTransactionResult(
    Guid Id,
    string TransactionType,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    DateTime CreatedAt,
    string? SourceEventId
);
