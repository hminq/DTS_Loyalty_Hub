using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Auth.Queries;
using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth.Handlers;

public sealed class GetCurrentAdminQueryHandler : IRequestHandler<GetCurrentAdminQuery, CurrentAdminResult>
{
    private readonly IAdminSessionRepository _adminSessionRepository;
    private readonly IAdminUserRepository _adminUserRepository;

    public GetCurrentAdminQueryHandler(
        IAdminSessionRepository adminSessionRepository,
        IAdminUserRepository adminUserRepository)
    {
        _adminSessionRepository = adminSessionRepository;
        _adminUserRepository = adminUserRepository;
    }

    public async Task<CurrentAdminResult> Handle(GetCurrentAdminQuery request, CancellationToken ct)
    {
        var isSessionActive = await _adminSessionRepository.IsSessionActiveAsync(
            request.AdminSessionId,
            request.AccessTokenJti,
            request.AdminId,
            request.UserId,
            ct);

        if (!isSessionActive)
        {
            throw new DomainException(
                "INVALID_ADMIN_SESSION",
                DomainErrorType.Unauthorized);
        }

        var admin = await _adminUserRepository.GetByIdAsync(request.AdminId, ct);

        if (admin is null || admin.UserId != request.UserId)
        {
            throw new DomainException(
                "INVALID_ADMIN_SESSION",
                DomainErrorType.Unauthorized);
        }

        return new CurrentAdminResult(
            admin.UserId,
            admin.AdminId,
            admin.Username,
            admin.Email,
            admin.FullName,
            admin.PhoneNumber,
            admin.RoleId,
            admin.RoleName,
            admin.Status,
            admin.Role?.Permissions.Select(permission => permission.Code).ToArray() ?? []);
    }
}
