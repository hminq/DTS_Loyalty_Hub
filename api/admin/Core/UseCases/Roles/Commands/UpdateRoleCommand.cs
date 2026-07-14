using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Commands;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    IReadOnlyCollection<Guid> PermissionIds) : IRequest<RoleResult>;
