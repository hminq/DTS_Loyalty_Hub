using Api.Dtos.Requests.CustomerVouchers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.CustomerVouchers;
using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerRedeems;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerVouchers;
using Core.UseCases.CustomerVouchers.Results;

namespace Api.Mappers;

public static class CustomerVoucherMapper
{
    public static GetCustomerVouchersQuery ToQuery(
        this GetCustomerVouchersRequestDto request,
        DateTime currentTime)
    {
        return new GetCustomerVouchersQuery(
            request.Page,
            request.PageSize,
            request.VoucherKeyword,
            request.RewardType,
            request.RedeemAtFrom,
            request.RedeemAtTo,
            request.UserKeyword,
            currentTime);
    }

    public static GetCustomerRedeemsQuery ToQuery(this GetCustomerRedeemsRequestDto request)
    {
        return new GetCustomerRedeemsQuery(
            request.Page,
            request.PageSize,
            request.VoucherKeyword,
            request.RewardType,
            request.RedeemAtFrom,
            request.RedeemAtTo,
            request.CampaignName,
            request.UserKeyword);
    }

    public static CustomerVoucherResponseDto ToResponseDto(this CustomerVoucherResult result)
    {
        return new CustomerVoucherResponseDto
        {
            CusVoucherId = result.CusVoucherId,
            CusInfo = result.CusInfo.ToResponseDto(),
            VoucherDefName = result.VoucherDefName,
            VoucherDefRewardType = result.VoucherDefRewardType,
            IsExpired = result.IsExpired
        };
    }

    public static CustomerVoucherDetailResponseDto ToResponseDto(this CustomerVoucherDetailResult result)
    {
        return new CustomerVoucherDetailResponseDto
        {
            CusVoucherId = result.CusVoucherId,
            CusInfo = result.CusInfo.ToResponseDto(),
            VoucherDefId = result.VoucherDefId,
            VoucherDefName = result.VoucherDefName,
            VoucherDefDescription = result.VoucherDefDescription,
            VoucherDefRewardType = result.VoucherDefRewardType,
            VoucherDefBannerImgUrl = result.VoucherDefBannerImgUrl,
            VoucherDefGenerationType = result.VoucherDefGenerationType,
            ValidFrom = result.ValidFrom,
            ValidTo = result.ValidTo,
            RedeemAt = result.RedeemAt
        };
    }

    public static CustomerRedeemResponseDto ToResponseDto(this CustomerRedeemResult result)
    {
        return new CustomerRedeemResponseDto
        {
            VoucherRedemptionId = result.VoucherRedemptionId,
            CusInfo = result.CusInfo.ToResponseDto(),
            CusVoucherId = result.CusVoucherId,
            VoucherDefId = result.VoucherDefId,
            VoucherDefName = result.VoucherDefName,
            VoucherDefDescription = result.VoucherDefDescription,
            CampaignId = result.CampaignId,
            CampaignName = result.CampaignName,
            ValidFrom = result.ValidFrom,
            ValidTo = result.ValidTo,
            RedeemAt = result.RedeemAt
        };
    }

    public static CustomerRedeemDetailResponseDto ToResponseDto(this CustomerRedeemDetailResult result)
    {
        return new CustomerRedeemDetailResponseDto
        {
            VoucherRedemptionId = result.VoucherRedemptionId,
            RedeemedAt = result.RedeemedAt,
            Customer = new CustomerRedeemCustomerResponseDto
            {
                CustomerId = result.Customer.CustomerId,
                Username = result.Customer.Username,
                Email = result.Customer.Email,
                Phone = result.Customer.Phone
            },
            Voucher = new CustomerRedeemVoucherResponseDto
            {
                CustomerVoucherId = result.Voucher.CustomerVoucherId,
                VoucherDefinitionId = result.Voucher.VoucherDefinitionId,
                VoucherPoolId = result.Voucher.VoucherPoolId,
                Name = result.Voucher.Name,
                Description = result.Voucher.Description,
                BannerImageUrl = result.Voucher.BannerImageUrl,
                VoucherCode = result.Voucher.VoucherCode,
                RewardType = result.Voucher.RewardType,
                RewardValue = result.Voucher.RewardValue,
                GenerationType = result.Voucher.GenerationType,
                ValidFrom = result.Voucher.ValidFrom,
                ValidTo = result.Voucher.ValidTo
            },
            IssuanceSource = new CustomerRedeemIssuanceSourceResponseDto
            {
                CampaignId = result.IssuanceSource.CampaignId,
                CampaignName = result.IssuanceSource.CampaignName,
                CampaignEventType = result.IssuanceSource.CampaignEventType,
                CampaignSessionId = result.IssuanceSource.CampaignSessionId,
                SessionStart = result.IssuanceSource.SessionStart,
                SessionEnd = result.IssuanceSource.SessionEnd,
                SessionStatus = result.IssuanceSource.SessionStatus,
                ActionId = result.IssuanceSource.ActionId,
                ActionType = result.IssuanceSource.ActionType
            }
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<CustomerVoucherResponseDto>> ToPagedResponseDto(
        this PagedResult<CustomerVoucherResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CustomerVoucherResponseDto>>
        {
            Data = result.Items.Select(item => item.ToResponseDto()).ToArray(),
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<CustomerRedeemResponseDto>> ToPagedResponseDto(
        this PagedResult<CustomerRedeemResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CustomerRedeemResponseDto>>
        {
            Data = result.Items.Select(item => item.ToResponseDto()).ToArray(),
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }

    private static CustomerInfoResponseDto ToResponseDto(this CustomerInfoResult result)
    {
        return new CustomerInfoResponseDto
        {
            CustomerId = result.CustomerId,
            CustomerUsername = result.CustomerUsername,
            CustomerEmail = result.CustomerEmail,
            CustomerPhone = result.CustomerPhone
        };
    }
}
