using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Tiers.Commands;
using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers.Handlers;

public sealed class CreateTierCommandHandler : IRequestHandler<CreateTierCommand, TierResult>
{
    private const string AuditLogCreateAction = "CREATE";

    private readonly ITierRepository _tierRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public CreateTierCommandHandler(
        ITierRepository tierRepository,
        IAuditLogWriter auditLogWriter)
    {
        _tierRepository = tierRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<TierResult> Handle(CreateTierCommand request, CancellationToken ct)
    {
        var tier = Tier.Create(
            request.Name,
            request.PointsRequired,
            request.CycleMonth,
            request.Priority);

        var existingTiers = await _tierRepository.GetListAsync(ct);

        ValidateTierName(tier, existingTiers);
        ValidatePriorityPointsOrder(tier, existingTiers);

        var createdTier = _tierRepository.Add(tier);

        _auditLogWriter.Add(
            new AuditLogEntry(
                request.ActorUserId,
                AuditLogCreateAction,
                AuditEntityTypes.TierConfig,
                createdTier.TierConfigId,
                null,                       
                JsonSerializer.Serialize(new
                {
                    tierConfigId = createdTier.TierConfigId,
                    name = createdTier.Name,
                    pointsRequired = createdTier.PointsRequired,
                    cycleMonth = createdTier.CycleMonth,
                    priority = createdTier.Priority,
                    createdAt = createdTier.CreatedAt
                }),                          
                null));

        return new TierResult(
            createdTier.TierConfigId,
            createdTier.Name,
            createdTier.PointsRequired,
            createdTier.CycleMonth,
            createdTier.Priority);
    }

    private static void ValidatePriorityPointsOrder(
        Tier tier,
        IReadOnlyCollection<TierResult> existingTiers)
    {
        if (existingTiers.Any(existingTier => existingTier.Priority == tier.Priority))
        {
            throw new DomainException(
                "TIER_PRIORITY_ALREADY_EXISTS",
                "A tier with this priority already exists.",
                DomainErrorType.Conflict);
        }

        var hasInvalidPointsOrder = existingTiers.Any(existingTier =>
            (existingTier.Priority > tier.Priority && existingTier.PointsRequired <= tier.PointsRequired) ||
            (existingTier.Priority < tier.Priority && existingTier.PointsRequired >= tier.PointsRequired));

        if (hasInvalidPointsOrder)
        {
            throw new DomainException(
                "TIER_POINTS_REQUIRED_INVALID",
                "Points required of a higher priority tier must be greater than points required of a lower priority tier.",
                DomainErrorType.Validation);
        }
    }

    private static void ValidateTierName(
        Tier tier,
        IReadOnlyCollection<TierResult> existingTiers)
    {
        if (existingTiers.Any(existingTier => 
            existingTier.Name.Equals(tier.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException(
                "TIER_NAME_ALREADY_EXISTS",
                "A tier with this name already exists.",
                DomainErrorType.Conflict);
        }
    }
}
