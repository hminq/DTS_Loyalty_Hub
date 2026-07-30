using Scheduler.Core.Abstractions;
using Scheduler.Core.Requests;
using MediatR;

namespace Scheduler.Core.Handlers;

public sealed class EndEligibleCampaignsBatchCommandHandler
    : IRequestHandler<EndEligibleCampaignsBatchCommand, EndEligibleCampaignsBatchResult>
{
    private readonly ICampaignSessionLifecycleStore _store;

    public EndEligibleCampaignsBatchCommandHandler(ICampaignSessionLifecycleStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<EndEligibleCampaignsBatchResult> Handle(
        EndEligibleCampaignsBatchCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ProcessedAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("ProcessedAt must be a UTC timestamp.", nameof(request));
        }

        if (request.BatchSize <= 0)
        {
            throw new ArgumentException("BatchSize must be greater than zero.", nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var eligibleCampaignIds = await _store.GetEligibleCampaignsForUpdateAsync(
            request.ProcessedAt,
            request.BatchSize,
            cancellationToken);

        if (eligibleCampaignIds.Count > 0)
        {
            await _store.EndCampaignsAsync(eligibleCampaignIds, request.ProcessedAt, cancellationToken);
        }

        return new EndEligibleCampaignsBatchResult(eligibleCampaignIds.Count, eligibleCampaignIds.Count);
    }
}
