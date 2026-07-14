using Core.UseCases.Common;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Queries;

public sealed record GetRolesQuery(
    int Page,
    int PageSize,
    string? Keyword) : IRequest<PagedResult<RoleResult>>;
