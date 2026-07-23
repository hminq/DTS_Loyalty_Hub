using Core.Entities;

namespace Core.Abstractions;

public interface IVoucherPoolMutationStore
{
    Task<int> CountPoolsAsync(
        Guid voucherDefinitionId,
        CancellationToken cancellationToken);

    Task<IReadOnlySet<string>> FindExistingCodesAsync(
        IReadOnlyCollection<string> voucherCodes,
        CancellationToken cancellationToken);

    Task BulkInsertPoolsAsync(
        IReadOnlyCollection<VoucherPoolMutation> pools,
        CancellationToken cancellationToken);
}
