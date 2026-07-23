using Api.Dtos.Responses.Vouchers;
using Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.Vouchers.Queries.GetCustomerVouchers;
using Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;

namespace Api.Mappers;

public static class VoucherMapper
{
    public static CustomerVoucherResponseDto ToResponseDto(this CustomerVoucherResult result)
    {
        return new CustomerVoucherResponseDto
        {
            CusVoucherId = result.CustomerVoucherId,
            VoucherDefName = result.VoucherDefinitionName,
            VoucherDefRewardType = result.VoucherDefinitionRewardType,
            IsExpired = result.IsExpired
        };
    }

    public static CustomerVoucherDetailResponseDto ToResponseDto(this CustomerVoucherDetailResult result)
    {
        return new CustomerVoucherDetailResponseDto
        {
            CusVoucherId = result.CustomerVoucherId,
            VoucherDefId = result.VoucherDefinitionId,
            VoucherDefName = result.VoucherDefinitionName,
            VoucherDefDescription = result.VoucherDefinitionDescription,
            VoucherDefRewardType = result.VoucherDefinitionRewardType,
            VoucherDefBannerImgUrl = result.VoucherDefinitionBannerImageUrl,
            ValidFrom = result.ValidFrom,
            ValidTo = result.ValidTo,
            RedeemAt = result.RedeemAt
        };
    }

    public static VoucherRedemptionResponseDto ToResponseDto(this VoucherRedemptionResult result)
    {
        return new VoucherRedemptionResponseDto
        {
            VoucherRedemptionId = result.VoucherRedemptionId,
            CusVoucherId = result.CustomerVoucherId,
            VoucherDefId = result.VoucherDefinitionId,
            VoucherDefName = result.VoucherDefinitionName,
            VoucherDefDescription = result.VoucherDefinitionDescription,
            CampaignId = result.CampaignId,
            CampaignName = result.CampaignName,
            ValidFrom = result.ValidFrom,
            ValidTo = result.ValidTo,
            RedeemAt = result.RedeemAt
        };
    }
}
