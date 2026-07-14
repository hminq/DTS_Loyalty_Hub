using MediatR;

namespace Core.UseCases.Roles.Commands;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest;
