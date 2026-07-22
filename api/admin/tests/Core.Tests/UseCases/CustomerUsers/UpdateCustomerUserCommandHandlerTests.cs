using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.CustomerUsers.Commands;
using Core.UseCases.CustomerUsers.Handlers;
using Core.UseCases.CustomerUsers.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.CustomerUsers;

public sealed class UpdateCustomerUserCommandHandlerTests
{
    private readonly Mock<ICustomerUserRepository> _customerRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    [Fact]
    public async Task Handle_ValidRequest_UpdatesProfileAndAddsCustomerAudit()
    {
        var customerId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var existing = CreateCustomer(customerId);
        _customerRepository.Setup(repository => repository.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new UpdateCustomerUserCommand(
                customerId,
                " New@Example.COM ",
                " New  Name ",
                " 0987654321 ",
                actorUserId),
            CancellationToken.None);

        result.Email.Should().Be("new@example.com");
        result.FullName.Should().Be("New Name");
        result.PhoneNumber.Should().Be("0987654321");
        result.Username.Should().Be(existing.Username);
        result.Tier.Should().Be(existing.Tier);
        result.CurrentTierPoint.Should().Be(existing.CurrentTierPoint);
        result.NextTierPoint.Should().Be(existing.NextTierPoint);
        _userRepository.Verify(repository => repository.UpdateCustomerProfileAsync(
            customerId,
            "new@example.com",
            "New Name",
            "0987654321",
            It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == actorUserId &&
            entry.Action == AuditActions.Update &&
            entry.EntityType == AuditEntityTypes.Customer &&
            entry.EntityId == customerId &&
            entry.OldValue != null && entry.OldValue.Contains(existing.Email) &&
            entry.NewValue != null && entry.NewValue.Contains("new@example.com") &&
            entry.Metadata == null)), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsBeforeMutationAndAudit()
    {
        var customerId = Guid.NewGuid();
        _customerRepository.Setup(repository => repository.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(customerId));
        _userRepository.Setup(repository => repository.EmailExistsExceptCustomerAsync(
                "duplicate@example.com",
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = () => CreateHandler().Handle(
            new UpdateCustomerUserCommand(customerId, "duplicate@example.com", null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("EMAIL_ALREADY_EXISTS");
        _userRepository.Verify(repository => repository.UpdateCustomerProfileAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _auditLogWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicatePhone_ThrowsBeforeMutationAndAudit()
    {
        var customerId = Guid.NewGuid();
        _customerRepository.Setup(repository => repository.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(customerId));
        _userRepository.Setup(repository => repository.PhoneNumberExistsExceptCustomerAsync(
                "0123456789",
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = () => CreateHandler().Handle(
            new UpdateCustomerUserCommand(customerId, "valid@example.com", null, "0123456789", null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("PHONE_NUMBER_ALREADY_EXISTS");
        _auditLogWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingCustomer_ThrowsNotFound()
    {
        var customerId = Guid.NewGuid();

        var action = () => CreateHandler().Handle(
            new UpdateCustomerUserCommand(customerId, "valid@example.com", null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_USER_NOT_FOUND");
    }

    [Fact]
    public void Command_ImplementsTransactionalWriteMarkers()
    {
        typeof(IWriteRequest).IsAssignableFrom(typeof(UpdateCustomerUserCommand)).Should().BeTrue();
        typeof(ITransactionalRequest).IsAssignableFrom(typeof(UpdateCustomerUserCommand)).Should().BeTrue();
    }

    private UpdateCustomerUserCommandHandler CreateHandler() => new(
        _customerRepository.Object,
        _userRepository.Object,
        _auditLogWriter.Object);

    private static CustomerUserDetailResult CreateCustomer(Guid customerId)
    {
        return new CustomerUserDetailResult(
            customerId,
            Guid.NewGuid(),
            "customer",
            "old@example.com",
            "Old Name",
            null,
            UserStatus.Enable,
            DateTime.UtcNow,
            100,
            200,
            new CustomerUserTierResult(Guid.NewGuid(), "Gold", 1000, 3, 2));
    }
}
