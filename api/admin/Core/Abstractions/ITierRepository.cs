using Core.Entities;
using Core.UseCases.Tiers.Results;

namespace Core.Abstractions;

public interface ITierRepository
{
    Task<IReadOnlyCollection<TierResult>> GetListAsync(
        CancellationToken ct);

    Task<Tier?> GetByIdAsync(
        Guid tierConfigId,
        CancellationToken ct);

    Tier Add(Tier tier);

    Task<Tier> UpdateAsync(
        Tier tier,
        CancellationToken ct);
}