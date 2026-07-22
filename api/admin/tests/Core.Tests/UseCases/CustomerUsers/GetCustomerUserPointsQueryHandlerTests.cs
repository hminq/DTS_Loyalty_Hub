using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.CustomerUsers.Handlers;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.CustomerUsers;

public sealed class GetCustomerUserPointsQueryHandlerTests
{
    private readonly Mock<ICustomerUserRepository> _repository = new();

    [Fact]
    public async Task Handle_ExistingWallet_ReturnsTierProgressAndBalances()
    {
        var customerId = Guid.NewGuid();
        var expected = new CustomerUserPointsResult(
            customerId,
            650,
            350,
            new CustomerUserTierResult(Guid.NewGuid(), "Gold", 1000, 3, 2),
            400,
            50,
            1200,
            600,
            150,
            DateTime.UtcNow);
        _repository.Setup(repository => repository.GetPointsAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetCustomerUserPointsQueryHandler(_repository.Object);

        var result = await handler.Handle(
            new GetCustomerUserPointsQuery(customerId),
            CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_ExistingCustomerWithoutWallet_ReturnsZeroBalances()
    {
        var customerId = Guid.NewGuid();
        var expected = new CustomerUserPointsResult(
            customerId,
            0,
            0,
            null,
            0,
            0,
            0,
            0,
            0,
            null);
        _repository.Setup(repository => repository.GetPointsAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetCustomerUserPointsQueryHandler(_repository.Object);

        var result = await handler.Handle(
            new GetCustomerUserPointsQuery(customerId),
            CancellationToken.None);

        result.ActivePoint.Should().Be(0);
        result.LockedPoint.Should().Be(0);
        result.LifetimePoint.Should().Be(0);
        result.SpentPoint.Should().Be(0);
        result.ExpiredPoint.Should().Be(0);
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MissingCustomer_ThrowsNotFound()
    {
        var customerId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetPointsAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerUserPointsResult?)null);
        var handler = new GetCustomerUserPointsQueryHandler(_repository.Object);

        var action = () => handler.Handle(
            new GetCustomerUserPointsQuery(customerId),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_USER_NOT_FOUND");
        exception.Which.ErrorType.Should().Be(DomainErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ThrowsBeforeRepositoryCall()
    {
        var handler = new GetCustomerUserPointsQueryHandler(_repository.Object);

        var action = () => handler.Handle(
            new GetCustomerUserPointsQuery(Guid.Empty),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_ID_REQUIRED");
        _repository.Verify(repository => repository.GetPointsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Query_DoesNotImplementWriteMarkers()
    {
        typeof(IWriteRequest).IsAssignableFrom(typeof(GetCustomerUserPointsQuery)).Should().BeFalse();
        typeof(ITransactionalRequest).IsAssignableFrom(typeof(GetCustomerUserPointsQuery)).Should().BeFalse();
    }
}
