using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Results;
using MediatR;

namespace Core.UseCases.AdminUsers;

public sealed class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, AdminUserResult>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AdminUserResult> Handle(CreateAdminUserCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var phoneNumber = NormalizeOptional(request.PhoneNumber);

        await EnsureRoleExistsAsync(request.RoleId, ct);

        if (await _adminUserRepository.UsernameExistsAsync(username, ct))
        {
            throw new DomainException(
                "USERNAME_ALREADY_EXISTS",
                "Username already exists.",
                DomainErrorType.Conflict);
        }

        if (await _adminUserRepository.EmailExistsAsync(email, ct))
        {
            throw new DomainException(
                "EMAIL_ALREADY_EXISTS",
                "Email already exists.",
                DomainErrorType.Conflict);
        }

        if (phoneNumber is not null && await _adminUserRepository.PhoneNumberExistsAsync(phoneNumber, ct))
        {
            throw new DomainException(
                "PHONE_NUMBER_ALREADY_EXISTS",
                "Phone number already exists.",
                DomainErrorType.Conflict);
        }

        return await _adminUserRepository.CreateAsync(
            username,
            email,
            _passwordHasher.Hash(request.Password),
            NormalizeOptional(request.FullName),
            phoneNumber,
            request.RoleId,
            ct);
    }

    private async Task EnsureRoleExistsAsync(Guid roleId, CancellationToken ct)
    {
        if (roleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                "Role id is required.",
                DomainErrorType.Validation);
        }

        if (!await _adminUserRepository.RoleExistsAsync(roleId, ct))
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                "Role does not exist.",
                DomainErrorType.Validation);
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
