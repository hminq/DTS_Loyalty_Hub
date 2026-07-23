using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Handlers;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerRedeems;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerVouchers;
using Core.UseCases.CustomerVouchers.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.CustomerVouchers;

public sealed class CustomerVoucherQueryHandlerTests
{
    private readonly Mock<ICustomerVoucherRepository> _repository = new();

    [Fact]
    public async Task GetVouchers_NormalizesFiltersAndForwardsCurrentTime()
    {
        var currentTime = new DateTime(2026, 7, 23, 10, 0, 0, DateTimeKind.Utc);
        _repository.Setup(repository => repository.GetPagedVouchersAsync(
                1,
                20,
                "Summer",
                "FIXED",
                null,
                null,
                "john",
                currentTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CustomerVoucherResult>([], 1, 20, 0));

        await new GetCustomerVouchersQueryHandler(_repository.Object).Handle(
            new GetCustomerVouchersQuery(
                1,
                20,
                " Summer ",
                " fixed ",
                null,
                null,
                " john ",
                currentTime),
            CancellationToken.None);

        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetRedeems_NormalizesFilters()
    {
        _repository.Setup(repository => repository.GetPagedRedeemsAsync(
                2,
                50,
                "Gift",
                "GIFT",
                null,
                null,
                "Campaign",
                "email@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CustomerRedeemResult>([], 2, 50, 0));

        await new GetCustomerRedeemsQueryHandler(_repository.Object).Handle(
            new GetCustomerRedeemsQuery(
                2,
                50,
                " Gift ",
                " gift ",
                null,
                null,
                " Campaign ",
                " email@example.com "),
            CancellationToken.None);

        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetVoucherDetail_WhenNotFound_ThrowsNotFound()
    {
        _repository.Setup(repository => repository.GetVoucherDetailAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerVoucherDetailResult?)null);

        var action = () => new GetCustomerVoucherDetailQueryHandler(_repository.Object)
            .Handle(new GetCustomerVoucherDetailQuery(Guid.NewGuid()), CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_VOUCHER_NOT_FOUND");
        exception.Which.ErrorType.Should().Be(DomainErrorType.NotFound);
    }

    [Fact]
    public async Task GetRedeems_InvalidDateRange_ThrowsValidationError()
    {
        var action = () => new GetCustomerRedeemsQueryHandler(_repository.Object)
            .Handle(
                new GetCustomerRedeemsQuery(
                    1,
                    20,
                    null,
                    null,
                    new DateTime(2026, 7, 2),
                    new DateTime(2026, 7, 1),
                    null,
                    null),
                CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("REDEEM_DATE_RANGE_INVALID");
        exception.Which.ErrorType.Should().Be(DomainErrorType.Validation);
        _repository.VerifyNoOtherCalls();
    }
}
