using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Handlers;

public sealed class GetCustomerUserPointsQueryHandler
    : IRequestHandler<GetCustomerUserPointsQuery, CustomerUserPointsResult>
{
    private readonly ICustomerUserRepository _customerUserRepository;

    public GetCustomerUserPointsQueryHandler(ICustomerUserRepository customerUserRepository)
    {
        _customerUserRepository = customerUserRepository;
    }

    public async Task<CustomerUserPointsResult> Handle(
        GetCustomerUserPointsQuery request,
        CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new DomainException("CUSTOMER_ID_REQUIRED", DomainErrorType.Validation);
        }

        return await _customerUserRepository.GetPointsAsync(request.CustomerId, ct)
            ?? throw new DomainException("CUSTOMER_USER_NOT_FOUND", DomainErrorType.NotFound);
    }
}
