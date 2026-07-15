using Core.Abstractions;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Customers.Queries.GetPointTransactions;

public sealed class GetPointTransactionsQueryHandler : IRequestHandler<GetPointTransactionsQuery, PagedResult<PointTransactionResult>?>
{
    private readonly IPointTransactionRepository _repository;

    public GetPointTransactionsQueryHandler(IPointTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PointTransactionResult>?> Handle(
        GetPointTransactionsQuery request,
        CancellationToken ct)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 || request.PageSize > 100 ? 20 : request.PageSize;

        return await _repository.GetPagedByCustomerIdAsync(
            request.CustomerId, 
            pageIndex, 
            pageSize, 
            request.TransactionType,
            request.FromDate,
            request.ToDate,
            request.MinAmount,
            request.MaxAmount,
            ct);
    }
}
