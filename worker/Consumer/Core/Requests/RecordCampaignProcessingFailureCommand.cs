using Consumer.Core.Abstractions;
using MediatR;

namespace Consumer.Core.Requests;

public sealed record RecordCampaignProcessingFailureCommand(
    Guid EventCampaignProcessingId,
    string OutcomeCode,
    string ErrorSummary)
    : IRequest<RecordCampaignProcessingFailureResult>,
      ITransactionalRequest;

public sealed record RecordCampaignProcessingFailureResult(
    Guid EventCampaignProcessingId,
    string Status,
    string? OutcomeCode,
    int AttemptCount,
    bool Updated);
