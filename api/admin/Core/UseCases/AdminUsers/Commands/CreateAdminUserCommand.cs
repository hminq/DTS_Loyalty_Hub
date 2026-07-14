using Core.UseCases.AdminUsers.Results;
using MediatR;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record CreateAdminUserCommand(
    string Username,
    string Email,
    string Password,
    string? FullName,
    string? PhoneNumber,
    Guid RoleId) : IRequest<AdminUserResult>;
