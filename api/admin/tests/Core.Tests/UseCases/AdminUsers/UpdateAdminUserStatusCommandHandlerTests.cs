using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Handlers;
using Core.UseCases.AdminUsers.Results;
using Core.UseCases.AuditLogs;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.AdminUsers;

public sealed class UpdateAdminUserStatusCommandHandlerTests
{
    private readonly Mock<IAdminUserRepository> _adminUserRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IAdminSessionRepository> _sessionRepository = new();
    private readonly Mock<IAuditLogWriter> _auditWriter = new();

    [Fact]
    public async Task Handle_DisableAdmin_UpdatesStatusRevokesSessionAndAddsAudit()
    {
        var adminId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        _adminUserRepository.Setup(x => x.GetByIdAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AdminResult(adminId, UserStatus.Enable));
        var handler = CreateHandler();

        await handler.Handle(
            new UpdateAdminUserStatusCommand(adminId, " disable ", actorId),
            CancellationToken.None);

        _userRepository.Verify(x => x.UpdateAdminStatusAsync(
            adminId, UserStatus.Disable, It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepository.Verify(x => x.RevokeActiveSessionsAsync(
            adminId, It.IsAny<CancellationToken>()), Times.Once);
        _auditWriter.Verify(x => x.Add(It.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == actorId &&
            entry.EntityType == AuditEntityTypes.Admin &&
            entry.EntityId == adminId &&
            entry.Action == "UPDATE_STATUS")), Times.Once);
    }

    [Fact]
    public async Task Handle_EnableAdmin_DoesNotRevokeSession()
    {
        var adminId = Guid.NewGuid();
        _adminUserRepository.Setup(x => x.GetByIdAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AdminResult(adminId, UserStatus.Disable));
        var handler = CreateHandler();

        await handler.Handle(
            new UpdateAdminUserStatusCommand(adminId, "enable", null),
            CancellationToken.None);

        _userRepository.Verify(x => x.UpdateAdminStatusAsync(
            adminId, UserStatus.Enable, It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepository.Verify(x => x.RevokeActiveSessionsAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AdminNotFound_ThrowsWithoutMutation()
    {
        var adminId = Guid.NewGuid();
        _adminUserRepository.Setup(x => x.GetByIdAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminUserResult?)null);
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new UpdateAdminUserStatusCommand(adminId, UserStatus.Disable, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("ADMIN_USER_NOT_FOUND");
        VerifyNoMutation();
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsBeforeRepositoryQuery()
    {
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new UpdateAdminUserStatusCommand(Guid.NewGuid(), "LOCKED", null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("ADMIN_STATUS_INVALID");
        _adminUserRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        VerifyNoMutation();
    }

    private UpdateAdminUserStatusCommandHandler CreateHandler() => new(
        _adminUserRepository.Object,
        _userRepository.Object,
        _sessionRepository.Object,
        _auditWriter.Object);

    private void VerifyNoMutation()
    {
        _userRepository.Verify(x => x.UpdateAdminStatusAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _sessionRepository.Verify(x => x.RevokeActiveSessionsAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _auditWriter.Verify(x => x.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    private static AdminUserResult AdminResult(Guid adminId, string status) => new(
        adminId,
        Guid.NewGuid(),
        "admin",
        "admin@example.com",
        "Admin User",
        null,
        Guid.NewGuid(),
        "Manager",
        status,
        DateTime.UtcNow);
}
