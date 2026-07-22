using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Queries;

public sealed record GetCustomerUserByIdQuery(Guid CustomerId)
    : IRequest<CustomerUserDetailResult>;
