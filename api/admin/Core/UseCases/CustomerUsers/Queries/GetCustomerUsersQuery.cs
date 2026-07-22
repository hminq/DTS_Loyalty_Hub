using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Queries;

public sealed record GetCustomerUsersQuery(
    int Page,
    int PageSize,
    string? Keyword,
    string? Status,
    Guid? TierId) : IRequest<PagedResult<CustomerUserListItemResult>>;
