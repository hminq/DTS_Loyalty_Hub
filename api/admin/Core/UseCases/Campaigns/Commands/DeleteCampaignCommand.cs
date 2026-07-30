using Core.Abstractions;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record DeleteCampaignCommand(
    Guid CampaignId,
    Guid? ActorUserId) : IRequest, ITransactionalRequest;
