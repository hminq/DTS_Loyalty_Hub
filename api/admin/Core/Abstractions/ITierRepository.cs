using Core.Entities;
using Core.UseCases.Tiers.Results;

namespace Core.Abstractions;

public interface ITierRepository
{
    Task<IReadOnlyCollection<TierResult>> GetListAsync(
        CancellationToken ct);

    Task<Tier> CreateAsync(Tier tier, CancellationToken ct = default);
}