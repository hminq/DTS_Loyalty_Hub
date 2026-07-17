using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Auth;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordVerifier> _passwordVerifier = new();
    private readonly Mock<IAccessTokenService> _accessTokenService = new();
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _sut = new RegisterCommandHandler(
            _userRepository.Object,
            _passwordVerifier.Object,
            _accessTokenService.Object);
    }

    private static RegisterCommand CreateCommand(
        string username = "john_doe",
        string email = "john@example.com",
        string password = "Pass1234",
        string fullName = "John Doe",
        string phone = "+84901234567")
    {
        return new RegisterCommand(username, email, password, fullName, phone);
    }

    private void SetupNoDuplicates()
    {
        _userRepository.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);
        _userRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);
        _userRepository.Setup(r => r.ExistsByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsRegisterResultWithAccessToken()
    {
        var command = CreateCommand();
        var created = new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid());
        var expiresAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        SetupNoDuplicates();

        _passwordVerifier.Setup(p => p.Hash(command.Password)).Returns("hashed-password");

        _userRepository
            .Setup(r => r.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.Is<NewCustomerUser>(u =>
                    u.Username == command.Username &&
                    u.Email == command.Email &&
                    u.PasswordHash == "hashed-password" &&
                    u.FullName == command.FullName &&
                    u.Phone == command.Phone)))
            .Returns(created);

        _accessTokenService.Setup(t => t.CreateExpiresAt()).Returns(expiresAt);
        _accessTokenService
            .Setup(t => t.CreateAccessToken(
                It.Is<CustomerTokenUser>(u =>
                    u.UserId == created.UserId &&
                    u.CustomerId == created.CustomerId &&
                    u.Username == command.Username),
                expiresAt))
            .Returns(new AccessToken("fake-jwt-token", expiresAt));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("fake-jwt-token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresAt.Should().Be(expiresAt);
        result.Customer.UserId.Should().Be(created.UserId);
        result.Customer.CustomerId.Should().Be(created.CustomerId);
        result.Customer.Username.Should().Be(command.Username);
        result.Customer.Email.Should().Be(command.Email);
        result.Customer.FullName.Should().Be(command.FullName);
    }

    [Fact]
    public async Task Handle_UsernameAlreadyExists_ThrowsDomainException()
    {
        var command = CreateCommand();

        _userRepository
            .Setup(r => r.ExistsByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("USERNAME_ALREADY_EXISTS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Conflict);

        _userRepository.Verify(
            r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            r => r.ExistsByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            r => r.Add(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NewCustomerUser>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ThrowsDomainException()
    {
        var command = CreateCommand();

        _userRepository
            .Setup(r => r.ExistsByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("EMAIL_ALREADY_EXISTS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Conflict);

        _userRepository.Verify(
            r => r.ExistsByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            r => r.Add(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NewCustomerUser>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PhoneAlreadyExists_ThrowsDomainException()
    {
        var command = CreateCommand();

        _userRepository
            .Setup(r => r.ExistsByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.ExistsByPhoneAsync(command.Phone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("PHONE_ALREADY_EXISTS");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Conflict);

        _userRepository.Verify(
            r => r.Add(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NewCustomerUser>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidRequest_ChecksDuplicatesInCorrectOrder()
    {
        var command = CreateCommand();
        var callOrder = new List<string>();

        _userRepository
            .Setup(r => r.ExistsByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("username"));
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("email"));
        _userRepository
            .Setup(r => r.ExistsByPhoneAsync(command.Phone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("phone"));

        _passwordVerifier.Setup(p => p.Hash(command.Password)).Returns("hashed-password");
        _userRepository
            .Setup(r => r.Add(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NewCustomerUser>()))
            .Returns(new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid()));
        _accessTokenService.Setup(t => t.CreateExpiresAt()).Returns(DateTime.UtcNow);
        _accessTokenService
            .Setup(t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), It.IsAny<DateTime>()))
            .Returns(new AccessToken("token", DateTime.UtcNow));

        await _sut.Handle(command, CancellationToken.None);

        callOrder.Should().Equal("username", "email", "phone");
    }

    [Fact]
    public async Task Handle_ValidRequest_HashesPasswordBeforeStoring()
    {
        var command = CreateCommand(password: "PlainTextPassword1");

        SetupNoDuplicates();

        _passwordVerifier.Setup(p => p.Hash("PlainTextPassword1")).Returns("hashed-value");

        _userRepository
            .Setup(r => r.Add(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NewCustomerUser>()))
            .Returns(new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid()));
        _accessTokenService.Setup(t => t.CreateExpiresAt()).Returns(DateTime.UtcNow);
        _accessTokenService
            .Setup(t => t.CreateAccessToken(It.IsAny<CustomerTokenUser>(), It.IsAny<DateTime>()))
            .Returns(new AccessToken("token", DateTime.UtcNow));

        await _sut.Handle(command, CancellationToken.None);

        _passwordVerifier.Verify(p => p.Hash("PlainTextPassword1"), Times.Once);
        _userRepository.Verify(
            r => r.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.Is<NewCustomerUser>(u => u.PasswordHash == "hashed-value" && u.PasswordHash != command.Password)),
            Times.Once);
    }
}
