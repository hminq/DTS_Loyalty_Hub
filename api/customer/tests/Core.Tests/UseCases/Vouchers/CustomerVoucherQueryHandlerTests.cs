using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.Vouchers.Queries.GetCustomerVouchers;
using Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Vouchers;

public sealed class CustomerVoucherQueryHandlerTests
{
    private readonly Mock<ICustomerVoucherRepository> _repository = new();

    [Fact]
    public async Task GetDetail_ForwardsCustomerOwnershipBoundary()
    {
        var customerId = Guid.NewGuid();
        var customerVoucherId = Guid.NewGuid();
        var expected = new CustomerVoucherDetailResult(
            customerVoucherId,
            Guid.NewGuid(),
            "Voucher",
            "Description",
            "FIXED",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow);
        _repository.Setup(repository => repository.GetVoucherDetailAsync(
                customerId,
                customerVoucherId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await new GetCustomerVoucherDetailQueryHandler(_repository.Object)
            .Handle(
                new GetCustomerVoucherDetailQuery(customerId, customerVoucherId),
                CancellationToken.None);

        result.Should().Be(expected);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetDetail_WhenVoucherIsNotOwnedByCustomer_ThrowsNotFound()
    {
        _repository.Setup(repository => repository.GetVoucherDetailAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerVoucherDetailResult?)null);

        var action = () => new GetCustomerVoucherDetailQueryHandler(_repository.Object)
            .Handle(
                new GetCustomerVoucherDetailQuery(Guid.NewGuid(), Guid.NewGuid()),
                CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CUSTOMER_VOUCHER_NOT_FOUND");
        exception.Which.ErrorType.Should().Be(DomainErrorType.NotFound);
    }

    [Fact]
    public async Task GetVouchers_NormalizesPagingTextAndUtcDates()
    {
        var customerId = Guid.NewGuid();
        var currentTime = new DateTime(2026, 7, 23, 10, 0, 0);
        var redeemAtFrom = new DateTime(2026, 7, 1);
        _repository.Setup(repository => repository.GetPagedVouchersAsync(
                customerId,
                1,
                20,
                "Voucher",
                "PERCENT",
                It.Is<DateTime?>(value => value!.Value.Kind == DateTimeKind.Utc),
                null,
                It.Is<DateTime>(value => value.Kind == DateTimeKind.Utc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CustomerVoucherResult>([], 0, 1, 20));

        await new GetCustomerVouchersQueryHandler(_repository.Object).Handle(
            new GetCustomerVouchersQuery(
                customerId,
                0,
                101,
                "  Voucher  ",
                " percent ",
                redeemAtFrom,
                null,
                currentTime),
            CancellationToken.None);

        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetRedemptions_NormalizesCampaignAndRewardFilters()
    {
        var customerId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetPagedRedemptionsAsync(
                customerId,
                2,
                50,
                "Gift",
                "GIFT",
                null,
                null,
                "Summer",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<VoucherRedemptionResult>([], 0, 2, 50));

        await new GetVoucherRedemptionsQueryHandler(_repository.Object).Handle(
            new GetVoucherRedemptionsQuery(
                customerId,
                2,
                50,
                " Gift ",
                " gift ",
                null,
                null,
                " Summer "),
            CancellationToken.None);

        _repository.VerifyAll();
    }
}
