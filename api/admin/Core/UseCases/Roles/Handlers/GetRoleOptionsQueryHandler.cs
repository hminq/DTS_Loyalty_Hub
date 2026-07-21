using Core.Abstractions;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Handlers;

public sealed class GetRoleOptionsQueryHandler
    : IRequestHandler<GetRoleOptionsQuery, IReadOnlyCollection<RoleOptionResult>>
{
    public const int MaxResults = 20;
    private readonly IRoleRepository _roleRepository;

    public GetRoleOptionsQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public Task<IReadOnlyCollection<RoleOptionResult>> Handle(
        GetRoleOptionsQuery request,
        CancellationToken ct)
    {
        return _roleRepository.SearchOptionsAsync(
            request.Keyword,
            MaxResults,
            ct);
    }
}
