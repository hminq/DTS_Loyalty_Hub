using Consumer.Core.Abstractions;
using MediatR;

namespace Consumer.Core.Requests;

public sealed record ProcessCampaignCommand(
    Guid EventCampaignProcessingId)
    : IRequest<ProcessCampaignResult>,
      ITransactionalRequest;

public sealed record ProcessCampaignResult(
    Guid EventCampaignProcessingId,
    string Status,
    string? OutcomeCode,
    int AttemptCount,
    int ExecutedActionCount);
