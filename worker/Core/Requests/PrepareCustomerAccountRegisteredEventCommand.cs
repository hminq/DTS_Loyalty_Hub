using Core.Abstractions;
using Core.Entities.Campaigns;
using MediatR;

namespace Core.Requests;

public sealed record PrepareCustomerAccountRegisteredEventCommand(
    ValidatedCustomerAccountRegisteredEvent CampaignEvent)
    : IRequest<PrepareCustomerAccountRegisteredEventResult>,
      ITransactionalRequest;

public sealed record PrepareCustomerAccountRegisteredEventResult(
    Guid EventId,
    string Status,
    bool Created,
    IReadOnlyList<PreparedCampaignTarget> Targets);
