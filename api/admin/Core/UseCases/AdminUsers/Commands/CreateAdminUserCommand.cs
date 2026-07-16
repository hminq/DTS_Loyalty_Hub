using Core.UseCases.AdminUsers.Results;
using MediatR;
using Core.Abstractions;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record CreateAdminUserCommand(
    string Username,
    string Email,
    string Password,
    string? FullName,
    string? PhoneNumber,
    Guid RoleId,
    Guid? ActorUserId) : IRequest<AdminUserResult>, ITransactionalRequest;
