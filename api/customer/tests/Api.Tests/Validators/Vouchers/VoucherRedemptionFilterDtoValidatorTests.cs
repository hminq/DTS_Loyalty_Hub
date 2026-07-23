using Api.Dtos.Requests.Vouchers;
using Api.Validators.Vouchers;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.Vouchers;

public sealed class VoucherRedemptionFilterDtoValidatorTests
{
    private readonly VoucherRedemptionFilterDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidFilter_HasNoErrors()
    {
        var request = new VoucherRedemptionFilterDto
        {
            Page = 1,
            PageSize = 20,
            VoucherKeyword = "Voucher",
            RewardType = "GIFT",
            CampaignName = "Summer",
            RedeemAtFrom = new DateTime(2026, 7, 1),
            RedeemAtTo = new DateTime(2026, 7, 31)
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_TooLongNames_HasCamelCaseValidationErrors()
    {
        var request = new VoucherRedemptionFilterDto
        {
            VoucherKeyword = new string('a', 201),
            CampaignName = new string('b', 201)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("voucherKeyword")
            .WithErrorCode("VOUCHER_NAME_TOO_LONG");
        result.ShouldHaveValidationErrorFor("campaignName")
            .WithErrorCode("CAMPAIGN_NAME_TOO_LONG");
    }

    [Fact]
    public void Validate_ReversedRedeemRange_HasValidationError()
    {
        var request = new VoucherRedemptionFilterDto
        {
            RedeemAtFrom = new DateTime(2026, 7, 31),
            RedeemAtTo = new DateTime(2026, 7, 1)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("redeemAtTo")
            .WithErrorCode("REDEEM_DATE_RANGE_INVALID");
    }
}
