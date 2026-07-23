using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Queries;

public sealed record GetCustomerUserPointTransactionsQuery(
    Guid CustomerId,
    int Page,
    int PageSize) : IRequest<PagedResult<CustomerPointTransactionResult>>;
