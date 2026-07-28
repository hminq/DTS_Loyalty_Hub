using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class FinalizeEventProcessingCommandHandler
    : IRequestHandler<
        FinalizeEventProcessingCommand,
        FinalizeEventProcessingResult>
{
    private readonly IEventProcessingFinalizationStore _store;
    private readonly TimeProvider _timeProvider;

    public FinalizeEventProcessingCommandHandler(
        IEventProcessingFinalizationStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<FinalizeEventProcessingResult> Handle(
        FinalizeEventProcessingCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID is required.", nameof(request));
        }

        var state = await _store.LockEventAsync(request.EventId, cancellationToken)
            ?? throw new CampaignProcessingException(
                CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
                retriable: true);

        var completedCount = state.ChildStatuses.Count(
            status => status == EventCampaignProcessingStatuses.Completed);
        var skippedCount = state.ChildStatuses.Count(
            status => status == EventCampaignProcessingStatuses.Skipped);
        var failedCount = state.ChildStatuses.Count(
            status => status == EventCampaignProcessingStatuses.Failed);
        var pendingCount = state.ChildStatuses.Count(
            status => status == EventCampaignProcessingStatuses.Pending);

        if (state.Status != EventProcessingStatuses.Pending)
        {
            return Result(state.Status, Updated: false);
        }

        var operationTime = _timeProvider.GetUtcNow().UtcDateTime;
        if (failedCount > 0)
        {
            _store.MarkEventFailed(state.EventId, operationTime);
            return Result(EventProcessingStatuses.Failed, Updated: true);
        }

        if (pendingCount > 0)
        {
            return Result(EventProcessingStatuses.Pending, Updated: false);
        }

        _store.MarkEventCompleted(state.EventId, operationTime);
        return Result(EventProcessingStatuses.Completed, Updated: true);

        FinalizeEventProcessingResult Result(string status, bool Updated)
        {
            return new FinalizeEventProcessingResult(
                state.EventId,
                status,
                completedCount,
                skippedCount,
                failedCount,
                pendingCount,
                Updated);
        }
    }
}
