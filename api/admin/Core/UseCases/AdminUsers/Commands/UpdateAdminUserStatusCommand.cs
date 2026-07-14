using MediatR;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record UpdateAdminUserStatusCommand(
    Guid AdminId,
    string Status) : IRequest;
