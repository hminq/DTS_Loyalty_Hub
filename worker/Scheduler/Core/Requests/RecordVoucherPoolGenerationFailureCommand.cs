using Scheduler.Core.Abstractions;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record RecordVoucherPoolGenerationFailureCommand(
    Guid JobId,
    string ErrorCode,
    string? ErrorDetails,
    bool Retriable,
    DateTime FailedAt) : IRequest, IWriteRequest;
