using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using MediatR;

namespace Core.UseCases.AdminUsers;

public sealed class RevokeAdminSessionCommandHandler : IRequestHandler<RevokeAdminSessionCommand>
{
    private readonly IAdminUserRepository _adminUserRepository;

    public RevokeAdminSessionCommandHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task Handle(RevokeAdminSessionCommand request, CancellationToken ct)
    {
        if (request.AdminId == Guid.Empty)
        {
            throw new DomainException(
                "ADMIN_ID_REQUIRED",
                "Admin id is required.",
                DomainErrorType.Validation);
        }

        var existingAdmin = await _adminUserRepository.GetByIdAsync(request.AdminId, ct);

        if (existingAdmin is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                "Admin user does not exist.",
                DomainErrorType.NotFound);
        }

        await _adminUserRepository.RevokeActiveSessionsAsync(request.AdminId, ct);
    }
}
