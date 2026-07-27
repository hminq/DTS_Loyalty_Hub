using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record EndEligibleCampaignsBatchCommand(
    DateTime ProcessedAt,
    int BatchSize)
    : IRequest<EndEligibleCampaignsBatchResult>,
      ITransactionalRequest;

public sealed record EndEligibleCampaignsBatchResult(
    int SelectedCount,
    int EndedCount)
{
    public bool HasMore(int batchSize) => SelectedCount == batchSize;
}
