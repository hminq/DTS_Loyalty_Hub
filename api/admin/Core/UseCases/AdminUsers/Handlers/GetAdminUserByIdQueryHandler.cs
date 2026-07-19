using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Queries;
using Core.UseCases.AdminUsers.Results;
using MediatR;

namespace Core.UseCases.AdminUsers.Handlers;

public sealed class GetAdminUserByIdQueryHandler : IRequestHandler<GetAdminUserByIdQuery, AdminUserResult>
{
    private readonly IAdminUserRepository _adminUserRepository;

    public GetAdminUserByIdQueryHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task<AdminUserResult> Handle(GetAdminUserByIdQuery request, CancellationToken ct)
    {
        if (request.AdminId == Guid.Empty)
        {
            throw new DomainException(
                "ADMIN_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var adminUser = await _adminUserRepository.GetByIdAsync(request.AdminId, ct);

        if (adminUser is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        return adminUser;
    }
}
