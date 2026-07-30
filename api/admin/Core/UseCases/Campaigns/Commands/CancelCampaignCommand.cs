using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record CancelCampaignCommand(
    Guid CampaignId,
    Guid? ActorUserId) : IRequest<CancelCampaignResult>, ITransactionalRequest;
