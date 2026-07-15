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

    public static IReadOnlyCollection<TierResponseDto> ToResponseDto(
        this IReadOnlyCollection<TierResult> results)
    {
        return results
            .Select(result => result.ToResponseDto())
            .ToArray();
    }
}
