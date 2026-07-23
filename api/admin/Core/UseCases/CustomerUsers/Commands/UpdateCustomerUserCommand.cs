using Core.Abstractions;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Commands;

public sealed record UpdateCustomerUserCommand(
    Guid CustomerId,
    string Email,
    string? FullName,
    string? PhoneNumber,
    Guid? ActorUserId) : IRequest<CustomerUserDetailResult>, ITransactionalRequest;
