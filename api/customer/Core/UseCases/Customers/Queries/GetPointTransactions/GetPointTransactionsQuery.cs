using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Customers.Queries.GetPointTransactions;

public sealed record GetPointTransactionsQuery(Guid CustomerId, int PageIndex, int PageSize) : IRequest<PagedResult<PointTransactionResult>?>;
