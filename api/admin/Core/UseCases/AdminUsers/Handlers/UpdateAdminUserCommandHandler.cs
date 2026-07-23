using Core.Abstractions;
using Core.Entities;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Results;
using MediatR;
using System.Text.Json;
using Core.UseCases.AuditLogs;
using Core.Entities.Constants;

namespace Core.UseCases.AdminUsers.Handlers;

public sealed class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, AdminUserResult>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IRoleReader _roleReader;
    private readonly IUserRepository _userRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IRoleReader roleReader,
        IUserRepository userRepository,
        IAdminRepository adminRepository,
        IAuditLogWriter auditLogWriter)
    {
        _adminUserRepository = adminUserRepository;
        _roleReader = roleReader;
        _userRepository = userRepository;
        _adminRepository = adminRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<AdminUserResult> Handle(UpdateAdminUserCommand request, CancellationToken ct)
    {
        if (request.AdminId == Guid.Empty)
        {
            throw new DomainException(
                "ADMIN_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var existingAdmin = await _adminUserRepository.GetByIdAsync(request.AdminId, ct);

        if (existingAdmin is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        if (request.RoleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var role = await _roleReader.GetDetailByIdAsync(request.RoleId, ct)
            ?? throw new DomainException(
                "ROLE_NOT_FOUND",
                DomainErrorType.Validation);

        var email = UserProfileRules.NormalizeEmail(request.Email);
        var phoneNumber = UserProfileRules.NormalizeOptionalPhoneNumber(request.PhoneNumber);

        if (await _userRepository.EmailExistsExceptAdminAsync(email, request.AdminId, ct))
        {
            throw new DomainException(
                "EMAIL_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }

        if (phoneNumber is not null &&
            await _userRepository.PhoneNumberExistsExceptAdminAsync(phoneNumber, request.AdminId, ct))
        {
            throw new DomainException(
                "PHONE_NUMBER_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }

        var fullName = UserProfileRules.NormalizeOptionalFullName(request.FullName);
        await _userRepository.UpdateAdminProfileAsync(request.AdminId, email, fullName, phoneNumber, ct);
        await _adminRepository.UpdateRoleAsync(request.AdminId, request.RoleId, ct);

        var updatedAdmin = new AdminUserResult(
            existingAdmin.AdminId,
            existingAdmin.UserId,
            existingAdmin.Username,
            email,
            fullName ?? string.Empty,
            phoneNumber,
            role.RoleId,
            role.Name,
            existingAdmin.Status,
            existingAdmin.CreatedAt,
            role);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, AuditActions.Update, AuditEntityTypes.Admin, request.AdminId,
            JsonSerializer.Serialize(new { existingAdmin.Email, existingAdmin.FullName, existingAdmin.PhoneNumber, existingAdmin.RoleId }),
            JsonSerializer.Serialize(new { email, fullName, phoneNumber, roleId = request.RoleId }), null));

        return updatedAdmin;
    }

}
