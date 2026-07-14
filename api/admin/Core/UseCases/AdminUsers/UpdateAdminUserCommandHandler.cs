using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Results;
using MediatR;

namespace Core.UseCases.AdminUsers;

public sealed class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, AdminUserResult>
{
    private readonly IAdminUserRepository _adminUserRepository;

    public UpdateAdminUserCommandHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task<AdminUserResult> Handle(UpdateAdminUserCommand request, CancellationToken ct)
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

        if (request.RoleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                "Role id is required.",
                DomainErrorType.Validation);
        }

        if (!await _adminUserRepository.RoleExistsAsync(request.RoleId, ct))
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                "Role does not exist.",
                DomainErrorType.Validation);
        }

        var email = request.Email.Trim();
        var phoneNumber = NormalizeOptional(request.PhoneNumber);

        if (await _adminUserRepository.EmailExistsExceptAsync(email, request.AdminId, ct))
        {
            throw new DomainException(
                "EMAIL_ALREADY_EXISTS",
                "Email already exists.",
                DomainErrorType.Conflict);
        }

        if (phoneNumber is not null &&
            await _adminUserRepository.PhoneNumberExistsExceptAsync(phoneNumber, request.AdminId, ct))
        {
            throw new DomainException(
                "PHONE_NUMBER_ALREADY_EXISTS",
                "Phone number already exists.",
                DomainErrorType.Conflict);
        }

        return await _adminUserRepository.UpdateAsync(
            request.AdminId,
            email,
            NormalizeOptional(request.FullName),
            phoneNumber,
            request.RoleId,
            ct);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
