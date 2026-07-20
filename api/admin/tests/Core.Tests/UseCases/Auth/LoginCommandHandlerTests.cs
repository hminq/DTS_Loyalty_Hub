using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Handlers;
using Core.UseCases.Auth.Models;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Auth;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IAdminSessionRepository> _sessionRepository = new();
    private readonly Mock<IPasswordVerifier> _passwordVerifier = new();
    private readonly Mock<IAccessTokenService> _accessTokenService = new();

    [Fact]
    public async Task Handle_ActiveSessionExists_ThrowsConflictWithoutCreatingToken()
    {
        var user = EnabledUser();
        SetupValidCredentials(user);
        _sessionRepository.Setup(repository => repository.CreateSessionIfNoneActiveAsync(
                user.AdminId,
                user.UserId,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminLoginSession?)null);
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new LoginCommand(user.Username, "correct-password"),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("SESSION_ALREADY_ACTIVE");
        exception.Which.ErrorType.Should().Be(DomainErrorType.Conflict);
        _accessTokenService.Verify(service => service.CreateAccessToken(
            It.IsAny<AdminLoginUser>(), It.IsAny<AdminLoginSession>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoActiveSession_CreatesSessionAndReturnsToken()
    {
        var user = EnabledUser();
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var session = new AdminLoginSession(Guid.NewGuid(), Guid.NewGuid(), expiresAt);
        SetupValidCredentials(user);
        _accessTokenService.Setup(service => service.CreateExpiresAt()).Returns(expiresAt);
        _sessionRepository.Setup(repository => repository.CreateSessionIfNoneActiveAsync(
                user.AdminId,
                user.UserId,
                expiresAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _accessTokenService.Setup(service => service.CreateAccessToken(user, session))
            .Returns(new AccessToken("access-token", expiresAt));
        var handler = CreateHandler();

        var result = await handler.Handle(
            new LoginCommand(user.Username, "correct-password"),
            CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.Admin.AdminId.Should().Be(user.AdminId);
    }

    private LoginCommandHandler CreateHandler() => new(
        _userRepository.Object,
        _sessionRepository.Object,
        _passwordVerifier.Object,
        _accessTokenService.Object);

    private void SetupValidCredentials(AdminLoginUser user)
    {
        _userRepository.Setup(repository => repository.GetByUsernameAsync(
                user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordVerifier.Setup(verifier => verifier.Verify(
                user.PasswordHash, "correct-password"))
            .Returns(true);
    }

    private static AdminLoginUser EnabledUser() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "admin",
        "Admin User",
        "password-hash",
        UserStatus.Enable,
        Guid.NewGuid(),
        "Administrator",
        ["dashboard.view"]);
}
