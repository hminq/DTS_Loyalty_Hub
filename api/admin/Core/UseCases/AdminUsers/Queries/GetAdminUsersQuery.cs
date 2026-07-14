using Core.UseCases.AdminUsers.Results;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.AdminUsers.Queries;

public sealed record GetAdminUsersQuery(
    int Page,
    int PageSize,
    string? Keyword,
    string? Status,
    Guid? RoleId) : IRequest<PagedResult<AdminUserResult>>;
