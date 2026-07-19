using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Auth;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordVerifier> _passwordVerifier = new();
    private readonly Mock<IAccessTokenService> _accessTokenService = new();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(
            _userRepository.Object,
            _passwordVerifier.Object,
            _accessTokenService.Object);
    }

    // ---------- Helper ----------

    private static CustomerLoginUser CreateUser(
        string username = "john",
        string passwordHash = "hashed-password",
        string status = "ENABLE")
    {
        return new CustomerLoginUser(
            UserId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Username: username,
            FullName: "John Doe",
            PasswordHash: passwordHash,
            Status: status);
    }

    // ---------- Happy path ----------

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResultWithAccessToken()
    {
        // Arrange
        var user = CreateUser();
        var expiresAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var accessToken = new AccessToken("fake-jwt-token", expiresAt);
        var command = new LoginCommand(user.Username, "CorrectPassword1");

        _userRepository
            .Setup(r => r.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordVerifier
            .Setup(p => p.Verify(user.PasswordHash, command.Password))
            .Returns(true);

        _accessTokenService
            .Setup(t => t.CreateExpiresAt())
            .Returns(expiresAt);

        _accessTokenService
            .Setup(t => t.CreateAccessToken(
                It.Is<CustomerTokenUser>(u =>
                    u.UserId == user.UserId &&
                    u.CustomerId == user.CustomerId &&
                    u.Username == user.Username),
                expiresAt))
            .Returns(accessToken);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("fake-jwt-token");
        result.ExpiresAt.Should().Be(expiresAt);
        result.Customer.UserId.Should().Be(user.UserId);
        result.Customer.CustomerId.Should().Be(user.CustomerId);
        result.Customer.Username.Should().Be(user.Username);
        result.Customer.FullName.Should().Be(user.FullName);

        _accessTokenService.Verify(
            t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), expiresAt),
            Times.Once);
    }

    // ---------- Unhappy path ----------

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var command = new LoginCommand("unknown-user", "AnyPassword1");

        _userRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerLoginUser?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("INVALID_CREDENTIALS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Unauthorized);

        // Không được đi tiếp tới bước tạo token khi user không tồn tại
        _accessTokenService.Verify(
            t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserIsDisabled_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser(status: "DISABLE");
        var command = new LoginCommand(user.Username, "CorrectPassword1");

        _userRepository
            .Setup(r => r.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("INVALID_CREDENTIALS");

        // User bị disable thì không được verify password (fail sớm, tránh lộ thông tin)
        // -> nhưng vì handler dùng short-circuit "||", password verifier vẫn có thể được gọi
        // tuỳ thứ tự điều kiện, nên chỉ assert không tạo access token là đủ an toàn.
        _accessTokenService.Verify(
            t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser();
        var command = new LoginCommand(user.Username, "WrongPassword1");

        _userRepository
            .Setup(r => r.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordVerifier
            .Setup(p => p.Verify(user.PasswordHash, command.Password))
            .Returns(false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("INVALID_CREDENTIALS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Unauthorized);

        _accessTokenService.Verify(
            t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserStatusEnabled_LoginSucceeds()
    {
        
        var user = CreateUser(status: "ENABLE");
        var command = new LoginCommand(user.Username, "CorrectPassword1");
        var expiresAt = DateTime.UtcNow.AddMinutes(30);

        _userRepository
            .Setup(r => r.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordVerifier
            .Setup(p => p.Verify(user.PasswordHash, command.Password))
            .Returns(true);

        _accessTokenService.Setup(t => t.CreateExpiresAt()).Returns(expiresAt);
        _accessTokenService
            .Setup(t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), expiresAt))
            .Returns(new AccessToken("fake-jwt-token", expiresAt));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var user = CreateUser();
        var command = new LoginCommand(user.Username, "CorrectPassword1");
        using var cts = new CancellationTokenSource();

        _userRepository
            .Setup(r => r.GetByUsernameAsync(user.Username, cts.Token))
            .ReturnsAsync(user);

        _passwordVerifier
            .Setup(p => p.Verify(user.PasswordHash, command.Password))
            .Returns(true);

        _accessTokenService.Setup(t => t.CreateExpiresAt()).Returns(DateTime.UtcNow);
        _accessTokenService
            .Setup(t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), It.IsAny<DateTime>()))
            .Returns(new AccessToken("token", DateTime.UtcNow));

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert: đúng CancellationToken được truyền xuyên suốt, không bị "quên" ở giữa chừng
        _userRepository.Verify(
            r => r.GetByUsernameAsync(user.Username, cts.Token),
            Times.Once);
    }
}
