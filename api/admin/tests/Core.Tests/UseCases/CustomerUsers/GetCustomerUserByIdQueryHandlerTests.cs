using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.CustomerUsers.Handlers;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.CustomerUsers;

public sealed class GetCustomerUserByIdQueryHandlerTests
{
    private readonly Mock<ICustomerUserRepository> _repository = new();

    [Fact]
    public async Task Handle_ExistingCustomer_ReturnsProfileAndTierProgress()
    {
        var customerId = Guid.NewGuid();
        var tier = new CustomerUserTierResult(Guid.NewGuid(), "Gold", 1000, 3, 2);
        var expected = new CustomerUserDetailResult(
            customerId,
            Guid.NewGuid(),
            "customer",
            "customer@example.com",
            "Customer User",
            "0123456789",
            "ENABLE",
            DateTime.UtcNow,
            650,
            350,
            tier);
        _repository.Setup(repository => repository.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetCustomerUserByIdQueryHandler(_repository.Object);

        var result = await handler.Handle(
            new GetCustomerUserByIdQuery(customerId),
            CancellationToken.None);

        result.Should().Be(expected);
        result.Tier.Should().Be(tier);
    }

    [Fact]
    public async Task Handle_MissingCustomer_ThrowsNotFound()
    {
        var customerId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerUserDetailResult?)null);
        var handler = new GetCustomerUserByIdQueryHandler(_repository.Object);

        var action = () => handler.Handle(
            new GetCustomerUserByIdQuery(customerId),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_USER_NOT_FOUND");
        exception.Which.ErrorType.Should().Be(DomainErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ThrowsBeforeRepositoryCall()
    {
        var handler = new GetCustomerUserByIdQueryHandler(_repository.Object);

        var action = () => handler.Handle(
            new GetCustomerUserByIdQuery(Guid.Empty),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_ID_REQUIRED");
        _repository.Verify(repository => repository.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Query_DoesNotImplementWriteMarkers()
    {
        typeof(IWriteRequest).IsAssignableFrom(typeof(GetCustomerUserByIdQuery)).Should().BeFalse();
        typeof(ITransactionalRequest).IsAssignableFrom(typeof(GetCustomerUserByIdQuery)).Should().BeFalse();
    }
}
