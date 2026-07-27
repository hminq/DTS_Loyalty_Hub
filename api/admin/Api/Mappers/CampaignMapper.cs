using System.Text.Json;
using Api.Dtos.Requests.Campaigns;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Campaigns;
using Campaign.Contracts.Constants;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using Core.UseCases.Common;

namespace Api.Mappers;

public static class CampaignMapper
{
    public static CreateCampaignCommand ToCreateCommand(
        this CreateCampaignRequestDto request,
        Guid? actorUserId)
    {
        return new CreateCampaignCommand(
            request.CampaignName,
            request.Description,
            request.BannerImageUrl,
            request.EventType,
            request.Condition.GetRawText(),
            request.StartDate,
            request.EndDate,
            request.ScheduleCron,
            request.DurationHour,
            request.UserLimitTotal,
            request.UserLimitSession,
            request.Actions?
                .Select(action => new CreateCampaignActionInput(
                    action.ActionType,
                    action.ActionConfig.GetRawText(),
                    action.ExecuteOrder,
                    action.TotalCount,
                    action.SessionCount))
                .ToArray() ?? [],
            actorUserId);
    }

    public static UpdateCampaignCommand ToUpdateCommand(
        this CampaignWriteRequestDto request,
        Guid campaignId,
        Guid? actorUserId)
    {
        return new UpdateCampaignCommand(
            campaignId,
            request.CampaignName,
            request.Description,
            request.BannerImageUrl,
            request.EventType,
            request.Condition.GetRawText(),
            request.StartDate,
            request.EndDate,
            request.ScheduleCron,
            request.DurationHour,
            request.UserLimitTotal,
            request.UserLimitSession,
            actorUserId);
    }

    public static CreateCampaignActionCommand ToCreateCommand(
        this CampaignActionWriteRequestDto request,
        Guid campaignId,
        Guid? actorUserId)
    {
        return new CreateCampaignActionCommand(
            campaignId,
            request.ActionType,
            request.ActionConfig.GetRawText(),
            request.ExecuteOrder,
            request.TotalCount,
            request.SessionCount,
            actorUserId);
    }

    public static UpdateCampaignActionCommand ToUpdateCommand(
        this CampaignActionWriteRequestDto request,
        Guid campaignId,
        Guid actionId,
        Guid? actorUserId)
    {
        return new UpdateCampaignActionCommand(
            campaignId,
            actionId,
            request.ActionType,
            request.ActionConfig.GetRawText(),
            request.ExecuteOrder,
            request.TotalCount,
            request.SessionCount,
            actorUserId);
    }

    public static GetCampaignsQuery ToQuery(this GetCampaignsRequestDto request)
    {
        return new GetCampaignsQuery(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.Status,
            request.EventType);
    }

    public static ApiResponseDto<IReadOnlyCollection<CampaignListItemResponseDto>> ToPagedResponseDto(
        this PagedResult<CampaignListItemResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CampaignListItemResponseDto>>
        {
            Data = result.Items.Select(item => new CampaignListItemResponseDto(
                item.CampaignId,
                item.CampaignName,
                item.EventType,
                item.Status,
                ToUtcOffset(item.StartDate),
                ToUtcOffset(item.EndDate),
                item.ScheduleCron,
                item.DurationHour,
                item.ActionCount,
                item.NextSessionStart.HasValue
                    ? ToUtcOffset(item.NextSessionStart.Value)
                    : null,
                ToUtcOffset(item.CreatedAt),
                ToUtcOffset(item.UpdatedAt)))
                .ToArray(),
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }

    public static CampaignDetailResponseDto ToResponseDto(this CampaignDetailResult result)
    {
        return new CampaignDetailResponseDto(
            result.CampaignId,
            result.CampaignName,
            result.Description,
            result.BannerImageKey,
            result.BannerImageUrl,
            result.EventType,
            ToUtcOffset(result.StartDate),
            ToUtcOffset(result.EndDate),
            JsonSerializer.Deserialize<JsonElement>(result.Condition),
            result.ScheduleCron,
            result.DurationHour,
            result.UserLimitTotal,
            result.UserLimitSession,
            result.Status,
            CampaignScheduleDefaults.TimeZone,
            ToUtcOffset(result.CreatedAt),
            ToUtcOffset(result.UpdatedAt),
            result.Actions.Select(action => new CampaignActionResponseDto(
                action.ActionId,
                action.ActionType,
                JsonSerializer.Deserialize<JsonElement>(action.ActionConfig),
                action.ExecuteOrder,
                action.TotalCount,
                action.SessionCount,
                action.UsedCount,
                ToUtcOffset(action.CreatedAt)))
                .ToArray(),
            result.Sessions.Select(session => new CampaignSessionResponseDto(
                session.CampaignSessionId,
                ToUtcOffset(session.SessionStart),
                ToUtcOffset(session.SessionEnd),
                session.Status,
                ToUtcOffset(session.CreatedAt),
                session.EndedAt.HasValue ? ToUtcOffset(session.EndedAt.Value) : null))
                .ToArray(),
            result.SessionCount);
    }

    public static CampaignActionResponseDto ToResponseDto(this CampaignActionResult action)
    {
        return new CampaignActionResponseDto(
            action.ActionId,
            action.ActionType,
            JsonSerializer.Deserialize<JsonElement>(action.ActionConfig),
            action.ExecuteOrder,
            action.TotalCount,
            action.SessionCount,
            action.UsedCount,
            ToUtcOffset(action.CreatedAt));
    }

    public static CampaignOptionsResponseDto ToResponseDto(this CampaignOptionsResult result)
    {
        return new CampaignOptionsResponseDto(
            result.CampaignStatuses.ToArray(),
            new CampaignScheduleOptionsResponseDto(
                result.Schedule.TimeZone,
                result.Schedule.DaysOfWeek.ToArray()),
            result.EventTypes.Select(eventType => new CampaignEventTypeOptionResponseDto(
                eventType.Code,
                new CampaignConditionOptionsResponseDto(
                    eventType.Condition.Options.Select(condition =>
                        new CampaignConditionOptionResponseDto(
                            condition.Code,
                            condition.Sources.ToArray(),
                            condition.Supported))
                        .ToArray()),
                eventType.ActionTypes.ToArray()))
                .ToArray(),
            result.ActionTypes.Select(actionType => new CampaignActionTypeOptionResponseDto(
                actionType.Code,
                actionType.CalculationTypes.Select(ToResponseDto).ToArray(),
                actionType.Recipients.Select(ToResponseDto).ToArray()))
                .ToArray());
    }

    private static CampaignCapabilityOptionResponseDto ToResponseDto(
        CampaignCapabilityOptionResult option)
    {
        return new CampaignCapabilityOptionResponseDto(option.Code, option.Supported);
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
