using Core.UseCases.Common;
using Core.UseCases.Customers.Queries.GetPointTransactions;

namespace Core.Abstractions;

public interface IPointTransactionRepository
{
    Task<PagedResult<PointTransactionResult>> GetPagedByCustomerIdAsync(Guid customerId, int pageIndex, int pageSize, CancellationToken ct);
}
