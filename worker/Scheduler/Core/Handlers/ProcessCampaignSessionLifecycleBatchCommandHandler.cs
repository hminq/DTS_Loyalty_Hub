using Campaign.Contracts.Constants;
using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities;
using Scheduler.Core.Requests;
using MediatR;

namespace Scheduler.Core.Handlers;

public sealed class ProcessCampaignSessionLifecycleBatchCommandHandler
    : IRequestHandler<ProcessCampaignSessionLifecycleBatchCommand, ProcessCampaignSessionLifecycleBatchResult>
{
    private readonly ICampaignSessionLifecycleStore _store;

    public ProcessCampaignSessionLifecycleBatchCommandHandler(ICampaignSessionLifecycleStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<ProcessCampaignSessionLifecycleBatchResult> Handle(
        ProcessCampaignSessionLifecycleBatchCommand request,
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

        var scheduledCandidates = await _store.GetDueScheduledSessionsForUpdateAsync(
            request.ProcessedAt,
            request.BatchSize,
            cancellationToken);

        var runningCandidates = await _store.GetDueRunningSessionsForUpdateAsync(
            request.ProcessedAt,
            request.BatchSize,
            cancellationToken);

        var mutations = new List<CampaignSessionLifecycleMutation>(scheduledCandidates.Count + runningCandidates.Count);
        int startedCount = 0;
        int endedCount = 0;

        foreach (var candidate in scheduledCandidates)
        {
            if (candidate.SessionEnd <= request.ProcessedAt)
            {
                mutations.Add(new CampaignSessionLifecycleMutation(
                    candidate.CampaignSessionId,
                    CampaignSessionStatuses.Ended,
                    request.ProcessedAt));
                endedCount++;
            }
            else if (candidate.SessionStart <= request.ProcessedAt && request.ProcessedAt < candidate.SessionEnd)
            {
                mutations.Add(new CampaignSessionLifecycleMutation(
                    candidate.CampaignSessionId,
                    CampaignSessionStatuses.Running,
                    null));
                startedCount++;
            }
        }

        foreach (var candidate in runningCandidates)
        {
            mutations.Add(new CampaignSessionLifecycleMutation(
                candidate.CampaignSessionId,
                CampaignSessionStatuses.Ended,
                request.ProcessedAt));
            endedCount++;
        }

        if (mutations.Count > 0)
        {
            await _store.ApplySessionMutationsAsync(mutations, cancellationToken);
        }

        return new ProcessCampaignSessionLifecycleBatchResult(
            scheduledCandidates.Count,
            runningCandidates.Count,
            startedCount,
            endedCount);
    }
}
