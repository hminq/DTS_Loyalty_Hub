using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record CreateCampaignActionCommand(
    Guid CampaignId,
    string ActionType,
    string ActionConfigJson,
    int ExecuteOrder,
    int? TotalCount,
    int? SessionCount,
    Guid? ActorUserId) : IRequest<CampaignActionResult>, ITransactionalRequest;
