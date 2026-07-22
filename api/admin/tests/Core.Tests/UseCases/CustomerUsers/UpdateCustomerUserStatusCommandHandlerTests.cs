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

public sealed class UpdateCustomerUserStatusCommandHandlerTests
{
    private readonly Mock<ICustomerUserRepository> _customerRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    [Fact]
    public async Task Handle_ValidStatus_UpdatesUserAndAddsCustomerAudit()
    {
        var customerId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        _customerRepository.Setup(repository => repository.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(customerId));
        var handler = CreateHandler();

        await handler.Handle(
            new UpdateCustomerUserStatusCommand(customerId, " disable ", actorUserId),
            CancellationToken.None);

        _userRepository.Verify(repository => repository.UpdateCustomerStatusAsync(
            customerId,
            UserStatus.Disable,
            It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == actorUserId &&
            entry.Action == AuditActions.UpdateStatus &&
            entry.EntityType == AuditEntityTypes.Customer &&
            entry.EntityId == customerId &&
            entry.OldValue != null && entry.OldValue.Contains(UserStatus.Enable) &&
            entry.NewValue != null && entry.NewValue.Contains(UserStatus.Disable))), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsBeforeReadOrMutation()
    {
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new UpdateCustomerUserStatusCommand(Guid.NewGuid(), "LOCKED", null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("STATUS_INVALID");
        _customerRepository.Verify(repository => repository.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(repository => repository.UpdateCustomerStatusAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingCustomer_ThrowsNotFoundWithoutAudit()
    {
        var customerId = Guid.NewGuid();

        var action = () => CreateHandler().Handle(
            new UpdateCustomerUserStatusCommand(customerId, UserStatus.Disable, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_USER_NOT_FOUND");
        _auditLogWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public void Command_ImplementsTransactionalWriteMarkers()
    {
        typeof(IWriteRequest).IsAssignableFrom(typeof(UpdateCustomerUserStatusCommand)).Should().BeTrue();
        typeof(ITransactionalRequest).IsAssignableFrom(typeof(UpdateCustomerUserStatusCommand)).Should().BeTrue();
    }

    private UpdateCustomerUserStatusCommandHandler CreateHandler() => new(
        _customerRepository.Object,
        _userRepository.Object,
        _auditLogWriter.Object);

    private static CustomerUserDetailResult CreateCustomer(Guid customerId)
    {
        return new CustomerUserDetailResult(
            customerId,
            Guid.NewGuid(),
            "customer",
            "customer@example.com",
            "Customer",
            null,
            UserStatus.Enable,
            DateTime.UtcNow,
            0,
            0,
            null);
    }
}
