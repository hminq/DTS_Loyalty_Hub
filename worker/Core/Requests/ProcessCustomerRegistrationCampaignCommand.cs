using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record ProcessCustomerRegistrationCampaignCommand(
    Guid EventCampaignProcessingId)
    : IRequest<ProcessCustomerRegistrationCampaignResult>,
      ITransactionalRequest;

public sealed record ProcessCustomerRegistrationCampaignResult(
    Guid EventCampaignProcessingId,
    string Status,
    string? OutcomeCode,
    int AttemptCount,
    int ExecutedActionCount);
