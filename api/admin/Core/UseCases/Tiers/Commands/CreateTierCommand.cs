using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers.Commands;

public sealed record CreateTierCommand(
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority,
    Guid? ActorUserId) : IRequest<TierResult>;