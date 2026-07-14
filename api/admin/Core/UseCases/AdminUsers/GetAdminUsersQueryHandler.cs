using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Queries;
using Core.UseCases.AdminUsers.Results;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.AdminUsers;

public sealed class GetAdminUsersQueryHandler
    : IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserResult>>
{
    private const int MaxPageSize = 100;
    private readonly IAdminUserRepository _adminUserRepository;

    public GetAdminUsersQueryHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public Task<PagedResult<AdminUserResult>> Handle(GetAdminUsersQuery request, CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException(
                "PAGE_INVALID",
                "Page must be greater than or equal to 1.",
                DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            throw new DomainException(
                "PAGE_SIZE_INVALID",
                $"Page size must be between 1 and {MaxPageSize}.",
                DomainErrorType.Validation);
        }

        return _adminUserRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.Status,
            request.RoleId,
            ct);
    }
}
