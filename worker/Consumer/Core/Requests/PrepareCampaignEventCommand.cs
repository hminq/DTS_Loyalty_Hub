using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using MediatR;

namespace Consumer.Core.Requests;

public sealed record PrepareCampaignEventCommand(
    IValidatedCampaignEvent CampaignEvent)
    : IRequest<PrepareCampaignEventResult>,
      ITransactionalRequest;

public sealed record PrepareCampaignEventResult(
    Guid EventId,
    string Status,
    bool Created,
    IReadOnlyList<PreparedCampaignTarget> Targets);
