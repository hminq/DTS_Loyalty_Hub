using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Handlers;

public sealed class GetCustomerUsersQueryHandler
    : IRequestHandler<GetCustomerUsersQuery, PagedResult<CustomerUserListItemResult>>
{
    private const int MaxPageSize = 100;
    private readonly ICustomerUserRepository _customerUserRepository;

    public GetCustomerUsersQueryHandler(ICustomerUserRepository customerUserRepository)
    {
        _customerUserRepository = customerUserRepository;
    }

    public Task<PagedResult<CustomerUserListItemResult>> Handle(
        GetCustomerUsersQuery request,
        CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException("PAGE_INVALID", DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            throw new DomainException("PAGE_SIZE_INVALID", DomainErrorType.Validation);
        }

        return _customerUserRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.Status,
            request.TierId,
            ct);
    }
}
