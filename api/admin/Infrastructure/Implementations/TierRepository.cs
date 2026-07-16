using Core.Abstractions;
using Core.Entities;
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

    public async Task<Tier> CreateAsync(Tier tier, CancellationToken ct = default)
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

        await _dbContext.SaveChangesAsync(ct);

        return tier;
    }
}
