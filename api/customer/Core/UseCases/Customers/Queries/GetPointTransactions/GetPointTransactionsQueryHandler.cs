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
        return await _repository.GetPagedByCustomerIdAsync(request.CustomerId, request.PageIndex, request.PageSize, ct);
    }
}
