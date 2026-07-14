using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Queries;

public sealed record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleResult>;
