using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Auth;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using Core.UseCases.Events.Models;
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
    private readonly Mock<IPublishedEventVersionRepository> _publishedEventVersionRepository = new();
    private readonly DateTimeOffset _occurredAt = new(2026, 7, 27, 8, 30, 0, TimeSpan.Zero);
    private readonly PublishedEventVersionReference _accountReference = new(
        Guid.NewGuid(),
        EventTypeCodes.CustomerAccountRegistered,
        1,
        EventRoutingKeys.CustomerAccountRegistered);
    private readonly PublishedEventVersionReference _referralReference = new(
        Guid.NewGuid(),
        EventTypeCodes.CustomerReferralSucceeded,
        1,
        EventRoutingKeys.CustomerReferralSucceeded);
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _sut = new RegisterCommandHandler(
            _userRepository.Object,
            _passwordVerifier.Object,
            _accessTokenService.Object,
            _outboxWriter.Object,
            _publishedEventVersionRepository.Object,
            new FixedTimeProvider(_occurredAt));
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsRegisterResultWithAccessTokenAndOneAccountEvent()
    {
        var command = CreateCommand();
        var created = new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid());
        var expiresAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        SetupNoDuplicates();
        SetupPublishedAccountEvent();
        SetupSuccessfulRegistration(command, created, expiresAt);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("fake-jwt-token");
        result.ExpiresAt.Should().Be(expiresAt);
        result.Customer.UserId.Should().Be(created.UserId);
        result.Customer.CustomerId.Should().Be(created.CustomerId);
        result.Customer.Username.Should().Be(command.Username);
        result.Customer.Email.Should().Be(command.Email);
        result.Customer.FullName.Should().Be(command.FullName);

        _publishedEventVersionRepository.Verify(
            repository => repository.GetPublishedAsync(
                EventTypeCodes.CustomerAccountRegistered,
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _publishedEventVersionRepository.Verify(
            repository => repository.GetPublishedAsync(
                EventTypeCodes.CustomerReferralSucceeded,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _outboxWriter.Verify(
            writer => writer.Add(
                It.Is<VersionedOutboxEvent<CustomerAccountRegisteredPayload>>(outboxEvent =>
                    outboxEvent.PublishedVersion == _accountReference &&
                    outboxEvent.Envelope.EventId != Guid.Empty &&
                    outboxEvent.Envelope.EventType == EventTypeCodes.CustomerAccountRegistered &&
                    outboxEvent.Envelope.EventVersion == 1 &&
                    outboxEvent.Envelope.OccurredAt == _occurredAt.UtcDateTime &&
                    outboxEvent.Envelope.Payload.UserId == created.UserId &&
                    outboxEvent.Envelope.Payload.CustomerId == created.CustomerId &&
                    outboxEvent.Envelope.Payload.Username == command.Username &&
                    outboxEvent.Envelope.Payload.FullName == command.FullName)),
            Times.Once);
        _outboxWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NewUser_NormalizesProfileBeforeCheckingStoringAndPublishing()
    {
        var command = CreateCommand(
            username: " customer01 ",
            email: " Customer@Example.COM ",
            fullName: " Nguyễn  Minh  Anh ",
            phone: " +84901234567 ");
        var created = new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid());
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        SetupNoDuplicates();
        SetupPublishedAccountEvent();
        SetupSuccessfulRegistration(
            command,
            created,
            expiresAt,
            expectedUsername: "customer01",
            expectedEmail: "customer@example.com",
            expectedFullName: "Nguyễn Minh Anh",
            expectedPhone: "+84901234567",
            token: "token");

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
        _outboxWriter.Verify(
            writer => writer.Add(
                It.Is<VersionedOutboxEvent<CustomerAccountRegisteredPayload>>(outboxEvent =>
                    outboxEvent.Envelope.Payload.Username == "customer01" &&
                    outboxEvent.Envelope.Payload.FullName == "Nguyễn Minh Anh")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidReferral_EmitsAccountAndReferralEventsWithIndependentFacts()
    {
        var command = CreateCommand(referralUsername: "referrer");
        var created = new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid());
        var referrer = new ReferralCustomer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "referrer",
            UserStatus.Enable);
        var outboxEvents = new List<object>();

        SetupNoDuplicates();
        SetupPublishedAccountEvent();
        SetupPublishedReferralEvent();
        SetupReferral(referrer);
        SetupSuccessfulRegistration(command, created, DateTime.UtcNow, token: "token");
        _outboxWriter
            .Setup(writer => writer.Add(It.IsAny<VersionedOutboxEvent<CustomerAccountRegisteredPayload>>()))
            .Callback<VersionedOutboxEvent<CustomerAccountRegisteredPayload>>(outboxEvents.Add);
        _outboxWriter
            .Setup(writer => writer.Add(It.IsAny<VersionedOutboxEvent<CustomerReferralSucceededPayload>>()))
            .Callback<VersionedOutboxEvent<CustomerReferralSucceededPayload>>(outboxEvents.Add);

        await _sut.Handle(command, CancellationToken.None);

        outboxEvents.Should().HaveCount(2);
        var accountEvent = outboxEvents[0].Should()
            .BeOfType<VersionedOutboxEvent<CustomerAccountRegisteredPayload>>()
            .Subject;
        var referralEvent = outboxEvents[1].Should()
            .BeOfType<VersionedOutboxEvent<CustomerReferralSucceededPayload>>()
            .Subject;

        accountEvent.PublishedVersion.Should().Be(_accountReference);
        referralEvent.PublishedVersion.Should().Be(_referralReference);
        accountEvent.Envelope.EventId.Should().NotBeEmpty();
        referralEvent.Envelope.EventId.Should().NotBeEmpty();
        accountEvent.Envelope.EventId.Should().NotBe(referralEvent.Envelope.EventId);
        accountEvent.Envelope.OccurredAt.Should().Be(_occurredAt.UtcDateTime);
        referralEvent.Envelope.OccurredAt.Should().Be(_occurredAt.UtcDateTime);
        accountEvent.Envelope.Payload.UserId.Should().Be(created.UserId);
        accountEvent.Envelope.Payload.CustomerId.Should().Be(created.CustomerId);
        accountEvent.Envelope.Payload.Username.Should().Be(command.Username);
        accountEvent.Envelope.Payload.FullName.Should().Be(command.FullName);
        referralEvent.Envelope.Payload.ReferrerCustomerId.Should().Be(referrer.CustomerId);
        referralEvent.Envelope.Payload.ReferredCustomerId.Should().Be(created.CustomerId);
        referralEvent.Envelope.Payload.ReferredUsername.Should().Be(command.Username);
    }

    [Fact]
    public async Task Handle_MissingAccountDefinition_ThrowsConfigurationExceptionBeforeCreatingUser()
    {
        var command = CreateCommand();

        SetupNoDuplicates();
        _publishedEventVersionRepository
            .Setup(repository => repository.GetPublishedAsync(
                EventTypeCodes.CustomerAccountRegistered,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublishedEventVersionReference?)null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<EventPublicationConfigurationException>();
        exception.Which.EventType.Should().Be(EventTypeCodes.CustomerAccountRegistered);
        exception.Which.EventVersion.Should().Be(1);
        VerifyNoUserOrOutboxWrite();
    }

    [Fact]
    public async Task Handle_MissingReferralDefinition_ThrowsConfigurationExceptionBeforeCreatingUser()
    {
        var command = CreateCommand(referralUsername: "referrer");
        var referrer = new ReferralCustomer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "referrer",
            UserStatus.Enable);

        SetupNoDuplicates();
        SetupReferral(referrer);
        SetupPublishedAccountEvent();
        _publishedEventVersionRepository
            .Setup(repository => repository.GetPublishedAsync(
                EventTypeCodes.CustomerReferralSucceeded,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublishedEventVersionReference?)null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<EventPublicationConfigurationException>();
        exception.Which.EventType.Should().Be(EventTypeCodes.CustomerReferralSucceeded);
        exception.Which.EventVersion.Should().Be(1);
        VerifyNoUserOrOutboxWrite();
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
        VerifyNoDefinitionLookupUserOrOutboxWrite();
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
        VerifyNoDefinitionLookupUserOrOutboxWrite();
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
        VerifyNoDefinitionLookupUserOrOutboxWrite();
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
        VerifyNoDefinitionLookupUserOrOutboxWrite();
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
        VerifyNoDefinitionLookupUserOrOutboxWrite();
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

        VerifyNoDefinitionLookupUserOrOutboxWrite();
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
        SetupPublishedAccountEvent();
        SetupSuccessfulRegistration(
            command,
            new CreatedCustomerUser(Guid.NewGuid(), Guid.NewGuid()),
            DateTime.UtcNow,
            token: "token");

        await _sut.Handle(command, CancellationToken.None);

        callOrder.Should().Equal("username", "email", "phone");
    }

    [Fact]
    public async Task Handle_ValidRequest_HashesPasswordBeforeStoring()
    {
        var command = CreateCommand(password: "PlainTextPassword1");

        SetupNoDuplicates();
        SetupPublishedAccountEvent();
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

    private void SetupPublishedAccountEvent()
    {
        _publishedEventVersionRepository
            .Setup(repository => repository.GetPublishedAsync(
                EventTypeCodes.CustomerAccountRegistered,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_accountReference);
    }

    private void SetupPublishedReferralEvent()
    {
        _publishedEventVersionRepository
            .Setup(repository => repository.GetPublishedAsync(
                EventTypeCodes.CustomerReferralSucceeded,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_referralReference);
    }

    private void SetupReferral(ReferralCustomer referrer)
    {
        _userRepository
            .Setup(repository => repository.GetReferralCustomerByUsernameAsync(
                referrer.Username,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(referrer);
    }

    private void SetupSuccessfulRegistration(
        RegisterCommand command,
        CreatedCustomerUser created,
        DateTime expiresAt,
        string expectedUsername = "john_doe",
        string expectedEmail = "john@example.com",
        string expectedFullName = "John Doe",
        string expectedPhone = "+84901234567",
        string token = "fake-jwt-token")
    {
        _passwordVerifier.Setup(p => p.Hash(command.Password)).Returns("hashed-password");

        _userRepository
            .Setup(r => r.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.Is<NewCustomerUser>(u =>
                    u.Username == expectedUsername &&
                    u.Email == expectedEmail &&
                    u.PasswordHash == "hashed-password" &&
                    u.FullName == expectedFullName &&
                    u.Phone == expectedPhone)))
            .Returns(created);

        _accessTokenService.Setup(t => t.CreateExpiresAt()).Returns(expiresAt);
        _accessTokenService
            .Setup(t => t.CreateAccessToken(
                It.Is<CustomerTokenUser>(u =>
                    u.UserId == created.UserId &&
                    u.CustomerId == created.CustomerId &&
                    u.Username == expectedUsername),
                expiresAt))
            .Returns(new AccessToken(token, expiresAt));
    }

    private void VerifyNoDefinitionLookupUserOrOutboxWrite()
    {
        _publishedEventVersionRepository.Verify(
            repository => repository.GetPublishedAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        VerifyNoUserOrOutboxWrite();
    }

    private void VerifyNoUserOrOutboxWrite()
    {
        _userRepository.Verify(
            repository => repository.Add(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<NewCustomerUser>()),
            Times.Never);
        _outboxWriter.Verify(
            writer => writer.Add(It.IsAny<VersionedOutboxEvent<CustomerAccountRegisteredPayload>>()),
            Times.Never);
        _outboxWriter.Verify(
            writer => writer.Add(It.IsAny<VersionedOutboxEvent<CustomerReferralSucceededPayload>>()),
            Times.Never);
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
