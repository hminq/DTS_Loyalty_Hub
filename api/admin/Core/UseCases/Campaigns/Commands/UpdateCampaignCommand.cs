using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Commands;

public sealed record UpdateCampaignCommand(
    Guid CampaignId,
    string CampaignName,
    string? Description,
    string? BannerImageUrl,
    string EventType,
    string ConditionJson,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string ScheduleCron,
    int DurationHour,
    int? UserLimitTotal,
    int? UserLimitSession,
    Guid? ActorUserId) : IRequest<CampaignDetailResult>, ITransactionalRequest;
