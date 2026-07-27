using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Auth;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Core.Tests.UseCases.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordVerifier> _passwordVerifier = new();
    private readonly Mock<IAccessTokenService> _accessTokenService = new();
    private readonly Mock<IOutboxWriter> _outboxWriter = new();
    private readonly DateTimeOffset _occurredAt = new(2026, 7, 27, 8, 30, 0, TimeSpan.Zero);
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _sut = new RegisterCommandHandler(
            _userRepository.Object,
            _passwordVerifier.Object,
            _accessTokenService.Object,
            _outboxWriter.Object,
            new FixedTimeProvider(_occurredAt));
    }

    private static RegisterCommand CreateCommand(
        string username = "john_doe",
        string email = "john@example.com",
        string password = "Pass1234",
        string fullName = "John Doe",
        string phone = "+84901234567",
        string? referralUsername = null)
    {
        return new RegisterCommand(username, email, password, fullName, phone, referralUsername);
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
        result.ExpiresAt.Should().Be(expiresAt);
        result.Customer.UserId.Should().Be(created.UserId);
        result.Customer.CustomerId.Should().Be(created.CustomerId);
        result.Customer.Username.Should().Be(command.Username);
        result.Customer.Email.Should().Be(command.Email);
        result.Customer.FullName.Should().Be(command.FullName);
        _outboxWriter.Verify(
            writer => writer.Add(
                It.Is<OutgoingEvent<CustomerAccountRegisteredData>>(outgoingEvent =>
                    outgoingEvent.EventId != Guid.Empty &&
                    outgoingEvent.EventType == EventTypeCodes.CustomerAccountRegistered &&
                    outgoingEvent.RoutingKey == EventRoutingKeys.CustomerAccountRegistered &&
                    outgoingEvent.OccurredAt == _occurredAt.UtcDateTime &&
                    outgoingEvent.Data.UserId == created.UserId &&
                    outgoingEvent.Data.CustomerId == created.CustomerId &&
                    outgoingEvent.Data.Source == CustomerRegistrationSources.Normal &&
                    outgoingEvent.Data.ReferrerCustomerId == null)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NewUser_NormalizesProfileBeforeCheckingAndStoring()
    {
        var command = CreateCommand(
            username: " customer01 ",
            email: " Customer@Example.COM ",
            fullName: " Nguyễn  Minh  Anh ",
            phone: " +84901234567 ");
        var created = new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid());
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        SetupNoDuplicates();
        _passwordVerifier.Setup(verifier => verifier.Hash(command.Password)).Returns("hashed-password");
        _userRepository
            .Setup(repository => repository.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.Is<NewCustomerUser>(user =>
                    user.Username == "customer01" &&
                    user.Email == "customer@example.com" &&
                    user.FullName == "Nguyễn Minh Anh" &&
                    user.Phone == "+84901234567")))
            .Returns(created);
        _accessTokenService.Setup(service => service.CreateExpiresAt()).Returns(expiresAt);
        _accessTokenService
            .Setup(service => service.CreateAccessToken(
                It.Is<CustomerTokenUser>(user => user.Username == "customer01"),
                expiresAt))
            .Returns(new AccessToken("token", expiresAt));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Customer.Username.Should().Be("customer01");
        result.Customer.Email.Should().Be("customer@example.com");
        result.Customer.FullName.Should().Be("Nguyễn Minh Anh");
        _userRepository.Verify(repository => repository.ExistsByEmailAsync(
            "customer@example.com",
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(repository => repository.ExistsByPhoneAsync(
            "+84901234567",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidReferral_EmitsReferralEventWithResolvedCustomerId()
    {
        var command = CreateCommand(referralUsername: "referrer");
        var created = new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid());
        var referrer = new ReferralCustomer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "referrer",
            UserStatus.Enable);

        SetupNoDuplicates();
        _userRepository
            .Setup(repository => repository.GetReferralCustomerByUsernameAsync(
                "referrer",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(referrer);
        _passwordVerifier.Setup(verifier => verifier.Hash(command.Password)).Returns("hash");
        _userRepository
            .Setup(repository => repository.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<NewCustomerUser>()))
            .Returns(created);
        _accessTokenService.Setup(service => service.CreateExpiresAt()).Returns(DateTime.UtcNow);
        _accessTokenService
            .Setup(service => service.CreateAccessToken(
                It.IsAny<CustomerTokenUser>(),
                It.IsAny<DateTime>()))
            .Returns(new AccessToken("token", DateTime.UtcNow));

        await _sut.Handle(command, CancellationToken.None);

        _outboxWriter.Verify(writer => writer.Add(
            It.Is<OutgoingEvent<CustomerAccountRegisteredData>>(outgoingEvent =>
                outgoingEvent.Data.Source == CustomerRegistrationSources.Referral &&
                outgoingEvent.Data.ReferrerCustomerId == referrer.CustomerId)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MissingReferral_RejectsRegistrationBeforeCreatingUser()
    {
        var command = CreateCommand(referralUsername: "missing_referrer");

        SetupNoDuplicates();
        _userRepository
            .Setup(repository => repository.GetReferralCustomerByUsernameAsync(
                "missing_referrer",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferralCustomer?)null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("REFERRAL_USERNAME_INVALID");
        exception.Which.ErrorType.Should().Be(DomainErrorType.Validation);
        _userRepository.Verify(
            repository => repository.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<NewCustomerUser>()),
            Times.Never);
        _outboxWriter.Verify(
            writer => writer.Add(It.IsAny<OutgoingEvent<CustomerAccountRegisteredData>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DisabledReferral_RejectsRegistrationBeforeCreatingUser()
    {
        var command = CreateCommand(referralUsername: "disabled_referrer");
        var disabledReferrer = new ReferralCustomer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "disabled_referrer",
            UserStatus.Disable);

        SetupNoDuplicates();
        _userRepository
            .Setup(repository => repository.GetReferralCustomerByUsernameAsync(
                "disabled_referrer",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(disabledReferrer);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("REFERRAL_USERNAME_INVALID");
        _userRepository.Verify(
            repository => repository.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<NewCustomerUser>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SelfReferral_RejectsWithoutResolvingReferralUsername()
    {
        var command = CreateCommand(
            username: "new_customer",
            referralUsername: "new_customer");

        SetupNoDuplicates();

        var act = () => _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("REFERRAL_USERNAME_INVALID");
        _userRepository.Verify(
            repository => repository.GetReferralCustomerByUsernameAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
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
        _outboxWriter.Verify(
            writer => writer.Add(It.IsAny<OutgoingEvent<CustomerAccountRegisteredData>>()),
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
        _outboxWriter.Verify(
            writer => writer.Add(It.IsAny<OutgoingEvent<CustomerAccountRegisteredData>>()),
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
        _outboxWriter.Verify(
            writer => writer.Add(It.IsAny<OutgoingEvent<CustomerAccountRegisteredData>>()),
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

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
