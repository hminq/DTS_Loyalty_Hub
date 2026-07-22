using Core.UseCases.Tiers.Results;
using MediatR;
using Core.Abstractions;

namespace Core.UseCases.Tiers.Commands;

public sealed record UpdateTierCommand(
    Guid TierConfigId,
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority,
    Guid? ActorUserId) : IRequest<TierResult>, ITransactionalRequest;