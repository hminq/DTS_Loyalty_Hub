using Scheduler.Core.Abstractions;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record ProcessCampaignSessionLifecycleBatchCommand(
    DateTime ProcessedAt,
    int BatchSize)
    : IRequest<ProcessCampaignSessionLifecycleBatchResult>,
      ITransactionalRequest;

public sealed record ProcessCampaignSessionLifecycleBatchResult(
    int ScheduledSelectedCount,
    int RunningSelectedCount,
    int StartedCount,
    int EndedCount)
{
    public bool HasMore(int batchSize) =>
        ScheduledSelectedCount == batchSize ||
        RunningSelectedCount == batchSize;
}
