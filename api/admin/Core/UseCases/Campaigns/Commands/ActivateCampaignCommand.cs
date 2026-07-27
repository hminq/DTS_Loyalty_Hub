using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record ActivateCampaignCommand(
    Guid CampaignId,
    Guid? ActorUserId) : IRequest<CampaignDetailResult>, ITransactionalRequest;
