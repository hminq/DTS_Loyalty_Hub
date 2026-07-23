using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Queries;

public sealed record GetCustomerUserVoucherRedemptionsQuery(
    Guid CustomerId,
    int Page,
    int PageSize) : IRequest<PagedResult<CustomerVoucherRedemptionResult>>;
