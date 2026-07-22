using System.Text.Json;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Tiers.Commands;
using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers.Handlers;

public sealed class UpdateTierCommandHandler : IRequestHandler<UpdateTierCommand, TierResult>
{
    private readonly ITierRepository _tierRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateTierCommandHandler(
        ITierRepository tierRepository,
        IAuditLogWriter auditLogWriter)
    {
        _tierRepository = tierRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<TierResult> Handle(UpdateTierCommand request, CancellationToken ct)
    {
        if (request.TierConfigId == Guid.Empty)
        {
            throw new DomainException(
                "TIER_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var tier = await _tierRepository.GetByIdAsync(request.TierConfigId, ct);

        if (tier is null)
        {
            throw new DomainException(
                "TIER_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        var oldValue = JsonSerializer.Serialize(new
        {
            name = tier.Name,
            pointsRequired = tier.PointsRequired,
            cycleMonth = tier.CycleMonth,
            priority = tier.Priority
        });

        tier.Update(
            request.Name,
            request.PointsRequired,
            request.CycleMonth,
            request.Priority);

        var otherTiers = (await _tierRepository.GetListAsync(ct))
            .Where(existingTier => existingTier.TierConfigId != tier.TierConfigId)
            .ToArray();

        ValidateTierName(tier.Name, otherTiers);
        ValidatePriorityPointsOrder(tier.PointsRequired, tier.Priority, otherTiers);

        var updatedTier = await _tierRepository.UpdateAsync(tier, ct);

        _auditLogWriter.Add(
            new AuditLogEntry(
                request.ActorUserId,
                AuditActions.Update,
                AuditEntityTypes.TierConfig,
                updatedTier.TierConfigId,
                oldValue,
                JsonSerializer.Serialize(new
                {
                    tierConfigId = updatedTier.TierConfigId,
                    name = updatedTier.Name,
                    pointsRequired = updatedTier.PointsRequired,
                    cycleMonth = updatedTier.CycleMonth,
                    priority = updatedTier.Priority
                }),
                null));

        return new TierResult(
            updatedTier.TierConfigId,
            updatedTier.Name,
            updatedTier.PointsRequired,
            updatedTier.CycleMonth,
            updatedTier.Priority);
    }

    private static void ValidatePriorityPointsOrder(
        decimal pointsRequired,
        int priority,
        IReadOnlyCollection<TierResult> otherTiers)
    {
        if (otherTiers.Any(existingTier => existingTier.Priority == priority))
        {
            throw new DomainException(
                "TIER_PRIORITY_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }

        var hasInvalidPointsOrder = otherTiers.Any(existingTier =>
            (existingTier.Priority > priority && existingTier.PointsRequired <= pointsRequired) ||
            (existingTier.Priority < priority && existingTier.PointsRequired >= pointsRequired));

        if (hasInvalidPointsOrder)
        {
            throw new DomainException(
                "TIER_POINTS_ORDER_INVALID",
                DomainErrorType.Validation);
        }
    }

    private static void ValidateTierName(
        string name,
        IReadOnlyCollection<TierResult> otherTiers)
    {
        if (otherTiers.Any(existingTier =>
            existingTier.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException(
                "TIER_NAME_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }
    }
}
