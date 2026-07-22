using Core.Abstractions;
using Core.UseCases.Roles.Handlers;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Roles;

public sealed class GetRoleOptionsQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepository = new();

    [Fact]
    public async Task Handle_SearchesWithFixedResultLimit()
    {
        const string keyword = "admin";
        IReadOnlyCollection<RoleOptionResult> expected =
        [
            new RoleOptionResult(Guid.NewGuid(), "System Admin")
        ];
        _roleRepository.Setup(repository => repository.SearchOptionsAsync(
                keyword,
                GetRoleOptionsQueryHandler.MaxResults,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetRoleOptionsQueryHandler(_roleRepository.Object);

        var result = await handler.Handle(new GetRoleOptionsQuery(keyword), CancellationToken.None);

        result.Should().BeSameAs(expected);
        _roleRepository.Verify(repository => repository.SearchOptionsAsync(
            keyword,
            20,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
