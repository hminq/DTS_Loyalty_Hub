using Core.UseCases.Roles.Results;
using MediatR;
using Core.Abstractions;

namespace Core.UseCases.Roles.Commands;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    IReadOnlyCollection<Guid> PermissionIds,
    Guid? ActorUserId) : IRequest<RoleResult>, ITransactionalRequest;
