using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record StartOrResumeVoucherPoolGenerationCommand(
    DateTime StartedAt) : IRequest<StartOrResumeVoucherPoolGenerationResult>, IWriteRequest;

public sealed record StartOrResumeVoucherPoolGenerationResult(
    bool HasWork,
    Guid? JobId,
    int ExpectedCount,
    int ProcessedCount,
    bool Failed)
{
    public static StartOrResumeVoucherPoolGenerationResult NoWork { get; } =
        new(false, null, 0, 0, false);
}
