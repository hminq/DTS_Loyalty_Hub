using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Handlers;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Roles;

public sealed class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IPermissionRepository> _permissionRepository = new();
    private readonly Mock<IAuditLogWriter> _auditWriter = new();

    [Fact]
    public async Task Handle_ValidCommand_AddsRoleAndAudit()
    {
        var permissionIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new CreateRoleCommand("Manager", permissionIds, Guid.NewGuid());
        _roleRepository.Setup(x => x.ExistsByNameAsync("Manager", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _permissionRepository
            .Setup(x => x.GetExistingIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionIds.ToHashSet());
        _roleRepository.Setup(x => x.Add(It.IsAny<Role>())).Returns((Role role) => role);
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Manager");
        result.PermissionIds.Should().BeEquivalentTo(permissionIds);
        _roleRepository.Verify(x => x.Add(It.IsAny<Role>()), Times.Once);
        _auditWriter.Verify(x => x.Add(It.Is<AuditLogEntry>(entry =>
            entry.EntityType == AuditEntityTypes.Role &&
            entry.EntityId == result.RoleId)), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicatedPermissionIds_ThrowsBeforeQueries()
    {
        var permissionId = Guid.NewGuid();
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new CreateRoleCommand("Manager", [permissionId, permissionId], null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("ROLE_PERMISSION_DUPLICATED");
        _roleRepository.Verify(x => x.ExistsByNameAsync(It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        VerifyNoMutation();
    }

    [Fact]
    public async Task Handle_MissingPermission_ThrowsWithoutMutation()
    {
        var permissionIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _roleRepository.Setup(x => x.ExistsByNameAsync("Manager", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _permissionRepository
            .Setup(x => x.GetExistingIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid> { permissionIds[0] });
        var handler = CreateHandler();

        var action = () => handler.Handle(
            new CreateRoleCommand("Manager", permissionIds, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("ROLE_PERMISSION_NOT_FOUND");
        VerifyNoMutation();
    }

    private CreateRoleCommandHandler CreateHandler() => new(
        _roleRepository.Object,
        _permissionRepository.Object,
        _auditWriter.Object);

    private void VerifyNoMutation()
    {
        _roleRepository.Verify(x => x.Add(It.IsAny<Role>()), Times.Never);
        _auditWriter.Verify(x => x.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }
}
