using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record FinalizeEventProcessingCommand(Guid EventId)
    : IRequest<FinalizeEventProcessingResult>,
      ITransactionalRequest;

public sealed record FinalizeEventProcessingResult(
    Guid EventId,
    string Status,
    int CompletedCampaignCount,
    int SkippedCampaignCount,
    int FailedCampaignCount,
    int PendingCampaignCount,
    bool Updated);
