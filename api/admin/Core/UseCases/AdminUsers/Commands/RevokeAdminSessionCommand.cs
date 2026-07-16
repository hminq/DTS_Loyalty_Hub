using MediatR;
using Core.Abstractions;

namespace Core.UseCases.AdminUsers.Commands;

public sealed record RevokeAdminSessionCommand(Guid AdminId, Guid? ActorUserId) : IRequest, ITransactionalRequest;
