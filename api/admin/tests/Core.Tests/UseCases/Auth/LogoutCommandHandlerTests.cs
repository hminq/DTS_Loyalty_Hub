using Core.Abstractions;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Handlers;
using Moq;

namespace Core.Tests.UseCases.Auth;

public sealed class LogoutCommandHandlerTests
{
    private readonly Mock<IAdminSessionRepository> _sessionRepository = new();

    [Fact]
    public async Task Handle_RevokesCurrentSession()
    {
        var command = new LogoutCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        var handler = new LogoutCommandHandler(_sessionRepository.Object);

        await handler.Handle(command, CancellationToken.None);

        _sessionRepository.Verify(repository => repository.RevokeSessionAsync(
            command.AdminId,
            command.AdminSessionId,
            command.AccessTokenJti,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
