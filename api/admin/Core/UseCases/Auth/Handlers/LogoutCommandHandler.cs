using Core.Abstractions;
using Core.UseCases.Auth.Commands;
using MediatR;

namespace Core.UseCases.Auth.Handlers;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IAdminSessionRepository _adminSessionRepository;

    public LogoutCommandHandler(IAdminSessionRepository adminSessionRepository)
    {
        _adminSessionRepository = adminSessionRepository;
    }

    public Task Handle(LogoutCommand request, CancellationToken ct)
    {
        return _adminSessionRepository.RevokeSessionAsync(
            request.AdminId,
            request.AdminSessionId,
            request.AccessTokenJti,
            ct);
    }
}
