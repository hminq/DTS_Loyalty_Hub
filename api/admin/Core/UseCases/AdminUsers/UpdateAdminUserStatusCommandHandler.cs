using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using MediatR;

namespace Core.UseCases.AdminUsers;

public sealed class UpdateAdminUserStatusCommandHandler : IRequestHandler<UpdateAdminUserStatusCommand>
{
    private readonly IAdminUserRepository _adminUserRepository;

    public UpdateAdminUserStatusCommandHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task Handle(UpdateAdminUserStatusCommand request, CancellationToken ct)
    {
        if (request.AdminId == Guid.Empty)
        {
            throw new DomainException(
                "ADMIN_ID_REQUIRED",
                "Admin id is required.",
                DomainErrorType.Validation);
        }

        var normalizedStatus = request.Status.Trim().ToUpperInvariant();

        if (normalizedStatus is not UserStatus.Enable and not UserStatus.Disable)
        {
            throw new DomainException(
                "ADMIN_STATUS_INVALID",
                "Admin status is invalid.",
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

        await _adminUserRepository.UpdateStatusAsync(request.AdminId, normalizedStatus, ct);
    }
}
