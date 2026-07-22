using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Handlers;

public sealed class GetCustomerUserPointTransactionsQueryHandler
    : IRequestHandler<GetCustomerUserPointTransactionsQuery, PagedResult<CustomerPointTransactionResult>>
{
    private const int MaxPageSize = 100;
    private readonly ICustomerUserRepository _customerUserRepository;

    public GetCustomerUserPointTransactionsQueryHandler(ICustomerUserRepository customerUserRepository)
    {
        _customerUserRepository = customerUserRepository;
    }

    public async Task<PagedResult<CustomerPointTransactionResult>> Handle(
        GetCustomerUserPointTransactionsQuery request,
        CancellationToken ct)
    {
        ValidateRequest(request.CustomerId, request.Page, request.PageSize);

        if (!await _customerUserRepository.ExistsAsync(request.CustomerId, ct))
        {
            throw new DomainException("CUSTOMER_USER_NOT_FOUND", DomainErrorType.NotFound);
        }

        return await _customerUserRepository.GetPointTransactionsAsync(
            request.CustomerId,
            request.Page,
            request.PageSize,
            ct);
    }

    private static void ValidateRequest(Guid customerId, int page, int pageSize)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("CUSTOMER_ID_REQUIRED", DomainErrorType.Validation);
        }

        if (page < 1)
        {
            throw new DomainException("PAGE_INVALID", DomainErrorType.Validation);
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new DomainException("PAGE_SIZE_INVALID", DomainErrorType.Validation);
        }
    }
}
