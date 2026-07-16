using Core.UseCases.AdminUsers.Results;
using MediatR;
using Core.Abstractions;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record UpdateAdminUserCommand(
    Guid AdminId,
    string Email,
    string? FullName,
    string? PhoneNumber,
    Guid RoleId,
    Guid? ActorUserId) : IRequest<AdminUserResult>, ITransactionalRequest;
