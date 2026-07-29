using Consumer.Core.Abstractions;
using MediatR;

namespace Consumer.Core.Requests;

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
