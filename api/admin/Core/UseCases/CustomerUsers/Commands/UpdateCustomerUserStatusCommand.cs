using Core.Abstractions;
using MediatR;

namespace Core.UseCases.CustomerUsers.Commands;

public sealed record UpdateCustomerUserStatusCommand(
    Guid CustomerId,
    string Status,
    Guid? ActorUserId) : IRequest, ITransactionalRequest;
