using Core.Abstractions;
using Core.Entities;
using Core.Exceptions;
using Core.UseCases.Tiers.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class TierRepository : ITierRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public TierRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TierResult>> GetListAsync(CancellationToken ct)
    {
       return await _dbContext.TiersConfigs
        .AsNoTracking()
        .OrderBy(x => x.Priority)
        .Select(x => new TierResult(
            x.TierConfigId,
            x.Name,
            x.PointsRequired,
            x.CycleMonth,
            x.Priority))
        .ToListAsync(ct);
    }

    public Tier Add(Tier tier)
    {
        _dbContext.TiersConfigs.Add(new TiersConfig
        {
            TierConfigId = tier.TierConfigId,
            Name = tier.Name,
            PointsRequired = tier.PointsRequired,
            CycleMonth = tier.CycleMonth,
            Priority = tier.Priority,
            CreatedAt = tier.CreatedAt
        });

        return tier;
    }

    public async Task<Tier?> GetByIdAsync(Guid tierConfigId, CancellationToken ct)
    {
        var tierConfig = await _dbContext.TiersConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TierConfigId == tierConfigId, ct);

        if (tierConfig is null)
        {
            return null;
        }

        return Tier.Restore(
            tierConfig.TierConfigId,
            tierConfig.Name,
            tierConfig.PointsRequired,
            tierConfig.CycleMonth,
            tierConfig.Priority,
            tierConfig.CreatedAt);
    }

    public async Task<Tier> UpdateAsync(Tier tier, CancellationToken ct)
    {
        var persistedTier = await _dbContext.TiersConfigs
            .FirstOrDefaultAsync(x => x.TierConfigId == tier.TierConfigId, ct);

        if (persistedTier is null)
        {
            throw new DomainException(
                "TIER_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        persistedTier.Name = tier.Name;
        persistedTier.PointsRequired = tier.PointsRequired;
        persistedTier.CycleMonth = tier.CycleMonth;
        persistedTier.Priority = tier.Priority;

        return tier;
    }
}