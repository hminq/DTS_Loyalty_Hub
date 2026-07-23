using Api.Dtos.Requests.Vouchers;
using Api.Validators.Vouchers;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.Vouchers;

public sealed class CustomerVoucherFilterDtoValidatorTests
{
    private readonly CustomerVoucherFilterDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidFilter_HasNoErrors()
    {
        var request = new CustomerVoucherFilterDto
        {
            Page = 1,
            PageSize = 100,
            VoucherKeyword = "Voucher",
            RewardType = "percent",
            RedeemAtFrom = new DateTime(2026, 7, 1),
            RedeemAtTo = new DateTime(2026, 7, 31)
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0, 20, "page", "PAGE_INVALID")]
    [InlineData(1, 0, "pageSize", "PAGE_SIZE_INVALID")]
    [InlineData(1, 101, "pageSize", "PAGE_SIZE_INVALID")]
    public void Validate_InvalidPaging_HasExpectedError(
        int page,
        int pageSize,
        string propertyName,
        string errorCode)
    {
        var request = new CustomerVoucherFilterDto
        {
            Page = page,
            PageSize = pageSize
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(propertyName)
            .WithErrorCode(errorCode);
    }

    [Fact]
    public void Validate_InvalidRewardType_HasValidationError()
    {
        var request = new CustomerVoucherFilterDto
        {
            RewardType = "UNKNOWN"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("rewardType")
            .WithErrorCode("VOUCHER_REWARD_TYPE_INVALID");
    }

    [Fact]
    public void Validate_ReversedRedeemRange_HasValidationError()
    {
        var request = new CustomerVoucherFilterDto
        {
            RedeemAtFrom = new DateTime(2026, 7, 31),
            RedeemAtTo = new DateTime(2026, 7, 1)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("redeemAtTo")
            .WithErrorCode("REDEEM_DATE_RANGE_INVALID");
    }
}
