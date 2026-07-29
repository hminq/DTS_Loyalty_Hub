namespace Scheduler.Core.Entities;

public sealed record VoucherPoolGenerationFailure(
    string ErrorCode,
    bool Retriable);
