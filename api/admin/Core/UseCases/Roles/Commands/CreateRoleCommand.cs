using Core.UseCases.Roles.Results;
using MediatR;
using Core.Abstractions;

namespace Core.UseCases.Roles.Commands;

public sealed record CreateRoleCommand(
    string Name,
    IReadOnlyCollection<Guid> PermissionIds,
    Guid? ActorUserId) : IRequest<RoleResult>, ITransactionalRequest;
