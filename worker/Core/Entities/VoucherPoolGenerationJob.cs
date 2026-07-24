namespace Core.Entities;

public sealed record VoucherPoolGenerationJob(
    Guid JobId,
    Guid VoucherDefinitionId,
    string JobType,
    int ExpectedCount,
    int ProcessedCount,
    string Status,
    int AttemptCount,
    int DefinitionTotalStock,
    int DefinitionRemainingStock,
    string DefinitionGenerationType,
    string DefinitionPublishType,
    DateTime? StartedAt,
    string? ImportFileKey = null);
