using MediatR;
using Core.Abstractions;

namespace Core.UseCases.Roles.Commands;

public sealed record DeleteRoleCommand(Guid RoleId, Guid? ActorUserId) : IRequest, ITransactionalRequest;
