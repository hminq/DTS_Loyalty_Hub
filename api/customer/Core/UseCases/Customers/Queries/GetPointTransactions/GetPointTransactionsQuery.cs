using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Customers.Queries.GetPointTransactions;

public sealed record GetPointTransactionsQuery(
    Guid CustomerId, 
    int PageIndex, 
    int PageSize,
    string? TransactionType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null
) : IRequest<PagedResult<PointTransactionResult>?>;
