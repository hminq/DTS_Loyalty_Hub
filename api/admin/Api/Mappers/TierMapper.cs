using Api.Dtos.Requests.Tiers;
using Api.Dtos.Responses.Tiers;
using Core.UseCases.Tiers.Commands;
using Core.UseCases.Tiers.Results;

namespace Api.Mappers;

public static class TierMapper
{
    public static CreateTierCommand ToCommand(this CreateTierRequestDto request, Guid? actorUserId)
    {
        return new CreateTierCommand(
            request.Name,
            request.PointsRequired,
            request.CycleMonth,
            request.Priority,
            actorUserId);
    }

    public static TierResponseDto ToResponseDto(this TierResult result)
    {
        return new TierResponseDto
        {
            TierConfigId = result.TierConfigId,
            Name = result.Name,
            PointsRequired = result.PointsRequired,
            CycleMonth = result.CycleMonth,
            Priority = result.Priority
        };
    }

    public static TierListItemResponseDto ToListItemResponseDto(this TierListItemResult result)
    {
        return new TierListItemResponseDto
        {
            TierConfigId = result.TierConfigId,
            Name = result.Name,
            PointsRequired = result.PointsRequired,
            Priority = result.Priority
        };
    }

    public static IReadOnlyCollection<TierListItemResponseDto> ToListItemResponseDto(
        this IReadOnlyCollection<TierListItemResult> results)
    {
        return results
            .Select(result => result.ToListItemResponseDto())
            .ToArray();
    }

    public static TierDetailResponseDto ToDetailResponseDto(this TierDetailResult result)
    {
        return new TierDetailResponseDto
        {
            TierConfigId = result.TierConfigId,
            Name = result.Name,
            PointsRequired = result.PointsRequired,
            CycleMonth = result.CycleMonth,
            Priority = result.Priority,
            CreatedAt = result.CreatedAt
        };
    }

    public static UpdateTierCommand ToCommand(
    this UpdateTierRequestDto request,
    Guid tierConfigId,
    Guid? actorUserId)
    {
        return new UpdateTierCommand(
            tierConfigId,
            request.Name,
            request.PointsRequired,
            request.CycleMonth,
            request.Priority,
            actorUserId);
    }
}
