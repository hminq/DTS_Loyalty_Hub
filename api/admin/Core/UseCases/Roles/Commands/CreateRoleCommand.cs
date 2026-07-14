using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Commands;

public sealed record CreateRoleCommand(
    string Name,
    IReadOnlyCollection<Guid> PermissionIds) : IRequest<RoleResult>;
