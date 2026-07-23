using Api.Dtos.Requests.CustomerVouchers;
using Api.Validators.CustomerVouchers;
using Core.Entities.Constants;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.CustomerVouchers;

public sealed class CustomerVoucherRequestDtoValidatorTests
{
    [Fact]
    public void GetCustomerVouchers_InvalidFilters_ReturnsCamelCaseErrors()
    {
        var request = new GetCustomerVouchersRequestDto
        {
            Page = 0,
            PageSize = 101,
            VoucherKeyword = new string('a', 101),
            RewardType = "UNKNOWN",
            RedeemAtFrom = new DateTime(2026, 7, 2),
            RedeemAtTo = new DateTime(2026, 7, 1),
            UserKeyword = new string('b', 101)
        };

        var result = new GetCustomerVouchersRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("page").WithErrorCode("PAGE_INVALID");
        result.ShouldHaveValidationErrorFor("pageSize").WithErrorCode("PAGE_SIZE_INVALID");
        result.ShouldHaveValidationErrorFor("voucherKeyword").WithErrorCode("KEYWORD_TOO_LONG");
        result.ShouldHaveValidationErrorFor("rewardType").WithErrorCode("VOUCHER_REWARD_TYPE_INVALID");
        result.ShouldHaveValidationErrorFor("redeemAtFrom").WithErrorCode("REDEEM_DATE_RANGE_INVALID");
        result.ShouldHaveValidationErrorFor("userKeyword").WithErrorCode("KEYWORD_TOO_LONG");
    }

    [Fact]
    public void GetCustomerRedeems_ValidFilters_HasNoErrors()
    {
        var request = new GetCustomerRedeemsRequestDto
        {
            Page = 1,
            PageSize = 20,
            VoucherKeyword = "summer",
            RewardType = VoucherRewardTypes.Fixed,
            RedeemAtFrom = new DateTime(2026, 7, 1),
            RedeemAtTo = new DateTime(2026, 7, 31),
            CampaignName = "campaign",
            UserKeyword = "john"
        };

        var result = new GetCustomerRedeemsRequestDtoValidator().TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
