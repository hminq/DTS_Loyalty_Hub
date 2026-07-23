using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Handlers;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.VoucherDefinitions;

public sealed class GetVoucherDefinitionsQueryHandlerTests
{
    private readonly Mock<IVoucherDefinitionRepository> _repository = new();

    [Fact]
    public async Task Handle_ValidRequest_CallsRepositoryAndReturnsPagedResult()
    {
        var item = new VoucherDefinitionListItemResult(
            Guid.NewGuid(),
            "WELCOME10",
            "Welcome Voucher",
            VoucherRewardTypes.Fixed,
            10,
            VoucherPublishTypes.Public,
            100,
            100,
            DateTime.UtcNow,
            null);

        var expectedResult = new PagedResult<VoucherDefinitionListItemResult>([item], 1, 20, 1);

        _repository.Setup(x => x.GetPagedAsync(1, 20, "welcome", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetVoucherDefinitionsQueryHandler(_repository.Object);
        var query = new GetVoucherDefinitionsQuery(1, 20, "welcome", null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expectedResult);
        _repository.Verify(x => x.GetPagedAsync(1, 20, "welcome", null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_InvalidPage_ThrowsDomainException(int invalidPage)
    {
        var handler = new GetVoucherDefinitionsQueryHandler(_repository.Object);
        var query = new GetVoucherDefinitionsQuery(invalidPage, 20, null, null, null, null);

        var action = () => handler.Handle(query, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("PAGE_INVALID");
        _repository.Verify(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Handle_InvalidPageSize_ThrowsDomainException(int invalidPageSize)
    {
        var handler = new GetVoucherDefinitionsQueryHandler(_repository.Object);
        var query = new GetVoucherDefinitionsQuery(1, invalidPageSize, null, null, null, null);

        var action = () => handler.Handle(query, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("PAGE_SIZE_INVALID");
        _repository.Verify(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
