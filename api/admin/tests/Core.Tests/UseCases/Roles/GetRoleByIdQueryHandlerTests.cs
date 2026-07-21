using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Handlers;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Roles;

public sealed class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepository = new();

    [Fact]
    public async Task Handle_ExistingRole_ReturnsPermissionMetadata()
    {
        var roleId = Guid.NewGuid();
        var permission = new RolePermissionDetailResult(
            Guid.NewGuid(),
            "role.view",
            "View Role",
            "role",
            "Role",
            10,
            10);
        var expected = new RoleDetailResult(
            roleId,
            "Role Manager",
            [permission],
            DateTime.UtcNow);
        _roleRepository.Setup(repository => repository.GetDetailByIdAsync(
                roleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetRoleByIdQueryHandler(_roleRepository.Object);

        var result = await handler.Handle(new GetRoleByIdQuery(roleId), CancellationToken.None);

        result.Should().Be(expected);
        result.Permissions.Should().ContainSingle().Which.Should().Be(permission);
    }

    [Fact]
    public async Task Handle_MissingRole_ThrowsNotFound()
    {
        var roleId = Guid.NewGuid();
        _roleRepository.Setup(repository => repository.GetDetailByIdAsync(
                roleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleDetailResult?)null);
        var handler = new GetRoleByIdQueryHandler(_roleRepository.Object);

        var action = () => handler.Handle(new GetRoleByIdQuery(roleId), CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("ROLE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_EmptyRoleId_ThrowsBeforeRepositoryQuery()
    {
        var handler = new GetRoleByIdQueryHandler(_roleRepository.Object);

        var action = () => handler.Handle(new GetRoleByIdQuery(Guid.Empty), CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("ROLE_ID_REQUIRED");
        _roleRepository.Verify(repository => repository.GetDetailByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
