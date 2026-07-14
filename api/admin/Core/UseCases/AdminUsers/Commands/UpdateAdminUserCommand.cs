using Core.UseCases.AdminUsers.Results;
using MediatR;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record UpdateAdminUserCommand(
    Guid AdminId,
    string Email,
    string? FullName,
    string? PhoneNumber,
    Guid RoleId) : IRequest<AdminUserResult>;
