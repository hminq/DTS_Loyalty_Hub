using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Handlers;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.CustomerUsers;

public sealed class GetCustomerUsersQueryHandlerTests
{
    private readonly Mock<ICustomerUserRepository> _repository = new();

    [Fact]
    public async Task Handle_ValidQuery_ReturnsPagedCustomers()
    {
        var tierId = Guid.NewGuid();
        var query = new GetCustomerUsersQuery(2, 10, "minh", "ENABLE", tierId);
        var customer = new CustomerUserListItemResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "minh",
            "minh@example.com",
            "Minh Nguyen",
            null,
            tierId,
            "Gold",
            "ENABLE",
            DateTime.UtcNow);
        var expected = new PagedResult<CustomerUserListItemResult>([customer], 2, 10, 11);
        _repository.Setup(repository => repository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Keyword,
                query.Status,
                query.TierId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetCustomerUsersQueryHandler(_repository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 20, "PAGE_INVALID")]
    [InlineData(1, 0, "PAGE_SIZE_INVALID")]
    [InlineData(1, 101, "PAGE_SIZE_INVALID")]
    public async Task Handle_InvalidPaging_ThrowsBeforeRepositoryCall(
        int page,
        int pageSize,
        string errorCode)
    {
        var handler = new GetCustomerUsersQueryHandler(_repository.Object);

        var action = () => handler.Handle(
            new GetCustomerUsersQuery(page, pageSize, null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be(errorCode);
        _repository.Verify(repository => repository.GetPagedAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Query_DoesNotImplementWriteMarkers()
    {
        typeof(IWriteRequest).IsAssignableFrom(typeof(GetCustomerUsersQuery)).Should().BeFalse();
        typeof(ITransactionalRequest).IsAssignableFrom(typeof(GetCustomerUsersQuery)).Should().BeFalse();
    }
}
