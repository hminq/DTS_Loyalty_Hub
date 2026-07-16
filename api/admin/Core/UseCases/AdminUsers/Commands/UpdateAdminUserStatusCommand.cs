using MediatR;
using Core.Abstractions;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record UpdateAdminUserStatusCommand(
    Guid AdminId,
    string Status,
    Guid? ActorUserId) : IRequest, ITransactionalRequest;
