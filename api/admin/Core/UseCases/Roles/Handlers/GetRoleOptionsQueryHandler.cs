using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Handlers;

public sealed class GetRoleOptionsQueryHandler
    : IRequestHandler<GetRoleOptionsQuery, PagedResult<RoleOptionResult>>
{
    private const int MaxPageSize = 100;
    private readonly IRoleRepository _roleRepository;

    public GetRoleOptionsQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public Task<PagedResult<RoleOptionResult>> Handle(
        GetRoleOptionsQuery request,
        CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException(
                "PAGE_INVALID",
                DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            throw new DomainException(
                "PAGE_SIZE_INVALID",
                DomainErrorType.Validation);
        }

        return _roleRepository.GetOptionsPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            ct);
    }
}
