using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Queries;

public sealed record GetCustomerUserPointsQuery(Guid CustomerId)
    : IRequest<CustomerUserPointsResult>;
