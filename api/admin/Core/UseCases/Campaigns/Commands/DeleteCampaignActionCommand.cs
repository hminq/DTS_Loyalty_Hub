using Core.Abstractions;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record DeleteCampaignActionCommand(
    Guid CampaignId,
    Guid ActionId,
    Guid? ActorUserId) : IRequest, ITransactionalRequest;
