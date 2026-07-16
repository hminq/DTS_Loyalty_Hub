using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Results;
using MediatR;
using System.Text.Json;
using Core.UseCases.AuditLogs;
using Core.Entities.Constants;

namespace Core.UseCases.AdminUsers.Handlers;

public sealed class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, AdminUserResult>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IUserRepository userRepository,
        IAdminRepository adminRepository,
        IPasswordHasher passwordHasher,
        IAuditLogWriter auditLogWriter)
    {
        _adminUserRepository = adminUserRepository;
        _userRepository = userRepository;
        _adminRepository = adminRepository;
        _passwordHasher = passwordHasher;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<AdminUserResult> Handle(CreateAdminUserCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var phoneNumber = NormalizeOptional(request.PhoneNumber);

        var role = await GetRoleAsync(request.RoleId, ct);

        if (await _userRepository.UsernameExistsAsync(username, ct))
        {
            throw new DomainException(
                "USERNAME_ALREADY_EXISTS",
                "Username already exists.",
                DomainErrorType.Conflict);
        }

        if (await _userRepository.EmailExistsAsync(email, ct))
        {
            throw new DomainException(
                "EMAIL_ALREADY_EXISTS",
                "Email already exists.",
                DomainErrorType.Conflict);
        }

        if (phoneNumber is not null && await _userRepository.PhoneNumberExistsAsync(phoneNumber, ct))
        {
            throw new DomainException(
                "PHONE_NUMBER_ALREADY_EXISTS",
                "Phone number already exists.",
                DomainErrorType.Conflict);
        }

        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var fullName = NormalizeOptional(request.FullName);

        _userRepository.AddAdminUser(
            userId, username, email, _passwordHasher.Hash(request.Password), fullName, phoneNumber, createdAt);
        _adminRepository.Add(adminId, userId, request.RoleId, createdAt);

        var createdAdmin = new AdminUserResult(
            adminId,
            userId,
            username,
            email,
            fullName ?? string.Empty,
            phoneNumber,
            role.RoleId,
            role.Name,
            UserStatus.Enable,
            createdAt,
            role);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, "CREATE", AuditEntityTypes.Admin, adminId, null,
            JsonSerializer.Serialize(new { adminId, userId, username, email, fullName, phoneNumber, roleId = request.RoleId, status = createdAdmin.Status }),
            null));

        return createdAdmin;
    }

    private async Task<AdminUserRoleResult> GetRoleAsync(Guid roleId, CancellationToken ct)
    {
        if (roleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                "Role id is required.",
                DomainErrorType.Validation);
        }

        return await _adminUserRepository.GetRoleByIdAsync(roleId, ct)
            ?? throw new DomainException(
                "ROLE_NOT_FOUND",
                "Role does not exist.",
                DomainErrorType.Validation);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
