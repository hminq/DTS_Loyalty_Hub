using MediatR;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record RevokeAdminSessionCommand(Guid AdminId) : IRequest;
