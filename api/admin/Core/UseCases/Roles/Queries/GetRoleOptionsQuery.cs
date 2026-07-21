using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Queries;

public sealed record GetRoleOptionsQuery(
    string? Keyword) : IRequest<IReadOnlyCollection<RoleOptionResult>>;
