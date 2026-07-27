using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record UpdateCampaignActionCommand(
    Guid CampaignId,
    Guid ActionId,
    string ActionType,
    string ActionConfigJson,
    int ExecuteOrder,
    int? TotalCount,
    int? SessionCount,
    decimal? TotalAmount,
    decimal? SessionAmount,
    Guid? ActorUserId) : IRequest<CampaignActionResult>, ITransactionalRequest;
