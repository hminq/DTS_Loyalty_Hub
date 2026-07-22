using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Handlers;

public sealed class GetCustomerUserByIdQueryHandler
    : IRequestHandler<GetCustomerUserByIdQuery, CustomerUserDetailResult>
{
    private readonly ICustomerUserRepository _customerUserRepository;

    public GetCustomerUserByIdQueryHandler(ICustomerUserRepository customerUserRepository)
    {
        _customerUserRepository = customerUserRepository;
    }

    public async Task<CustomerUserDetailResult> Handle(
        GetCustomerUserByIdQuery request,
        CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new DomainException("CUSTOMER_ID_REQUIRED", DomainErrorType.Validation);
        }

        var customer = await _customerUserRepository.GetByIdAsync(request.CustomerId, ct);

        if (customer is null)
        {
            throw new DomainException("CUSTOMER_USER_NOT_FOUND", DomainErrorType.NotFound);
        }

        return customer;
    }
}
