namespace Core.UseCases.VoucherDefinitions.Results;

public sealed record VoucherPoolProvisioningResult(
    Guid JobId,
    string JobType,
    string Status,
    int ExpectedCount,
    int ProcessedCount,
    int AttemptCount,
    string? ErrorCode,
    string? ErrorDetails,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);
