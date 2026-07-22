using Api.Dtos.Requests.CustomerUsers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.CustomerUsers;
using Core.Entities;
using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Commands;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;

namespace Api.Mappers;

public static class CustomerUserMapper
{
    public static UpdateCustomerUserCommand ToCommand(
        this UpdateCustomerUserRequestDto request,
        Guid customerId,
        Guid? actorUserId)
    {
        return new UpdateCustomerUserCommand(
            customerId,
            UserProfileRules.NormalizeEmail(request.Email!),
            UserProfileRules.NormalizeOptionalFullName(request.FullName),
            UserProfileRules.NormalizeOptionalPhoneNumber(request.PhoneNumber),
            actorUserId);
    }

    public static UpdateCustomerUserStatusCommand ToCommand(
        this UpdateCustomerUserStatusRequestDto request,
        Guid customerId,
        Guid? actorUserId)
    {
        return new UpdateCustomerUserStatusCommand(
            customerId,
            request.Status!.Trim(),
            actorUserId);
    }

    public static GetCustomerUsersQuery ToQuery(this GetCustomerUsersRequestDto request)
    {
        return new GetCustomerUsersQuery(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.Status,
            request.TierId);
    }

    public static ApiResponseDto<IReadOnlyCollection<CustomerUserListItemResponseDto>> ToPagedResponseDto(
        this PagedResult<CustomerUserListItemResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CustomerUserListItemResponseDto>>
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

    public static CustomerUserListItemResponseDto ToResponseDto(
        this CustomerUserListItemResult result)
    {
        return new CustomerUserListItemResponseDto
        {
            CustomerId = result.CustomerId,
            UserId = result.UserId,
            Username = result.Username,
            Email = result.Email,
            FullName = result.FullName,
            PhoneNumber = result.PhoneNumber,
            TierId = result.TierId,
            TierName = result.TierName,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };
    }

    public static CustomerUserResponseDto ToResponseDto(this CustomerUserDetailResult result)
    {
        return new CustomerUserResponseDto
        {
            CustomerId = result.CustomerId,
            UserId = result.UserId,
            Username = result.Username,
            Email = result.Email,
            FullName = result.FullName,
            PhoneNumber = result.PhoneNumber,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            CurrentTierPoint = result.CurrentTierPoint,
            NextTierPoint = result.NextTierPoint,
            Tier = result.Tier is null
                ? null
                : new CustomerUserTierResponseDto
                {
                    TierId = result.Tier.TierId,
                    Name = result.Tier.Name,
                    PointsRequired = result.Tier.PointsRequired,
                    CycleMonth = result.Tier.CycleMonth,
                    Priority = result.Tier.Priority
                }
        };
    }

    public static CustomerUserPointsResponseDto ToResponseDto(this CustomerUserPointsResult result)
    {
        return new CustomerUserPointsResponseDto
        {
            CustomerId = result.CustomerId,
            CurrentTierPoint = result.CurrentTierPoint,
            NextTierPoint = result.NextTierPoint,
            Tier = result.Tier is null
                ? null
                : new CustomerUserTierResponseDto
                {
                    TierId = result.Tier.TierId,
                    Name = result.Tier.Name,
                    PointsRequired = result.Tier.PointsRequired,
                    CycleMonth = result.Tier.CycleMonth,
                    Priority = result.Tier.Priority
                },
            ActivePoint = result.ActivePoint,
            LockedPoint = result.LockedPoint,
            LifetimePoint = result.LifetimePoint,
            SpentPoint = result.SpentPoint,
            ExpiredPoint = result.ExpiredPoint,
            UpdatedAt = result.UpdatedAt
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<CustomerVoucherResponseDto>> ToPagedResponseDto(
        this PagedResult<CustomerVoucherResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CustomerVoucherResponseDto>>
        {
            Data = result.Items.Select(item => item.ToResponseDto()).ToArray(),
            Meta = result.ToMetaDto()
        };
    }

    public static CustomerVoucherResponseDto ToResponseDto(this CustomerVoucherResult result)
    {
        return new CustomerVoucherResponseDto
        {
            CustomerVoucherId = result.CustomerVoucherId,
            VoucherDefinitionId = result.VoucherDefinitionId,
            VoucherDefinitionName = result.VoucherDefinitionName,
            VoucherPoolId = result.VoucherPoolId,
            VoucherCode = result.VoucherCode,
            ValidFrom = result.ValidFrom,
            ValidTo = result.ValidTo,
            RemainingCount = result.RemainingCount,
            ReceivedAt = result.ReceivedAt
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<CustomerVoucherRedemptionResponseDto>> ToPagedResponseDto(
        this PagedResult<CustomerVoucherRedemptionResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CustomerVoucherRedemptionResponseDto>>
        {
            Data = result.Items.Select(item => item.ToResponseDto()).ToArray(),
            Meta = result.ToMetaDto()
        };
    }

    public static CustomerVoucherRedemptionResponseDto ToResponseDto(
        this CustomerVoucherRedemptionResult result)
    {
        return new CustomerVoucherRedemptionResponseDto
        {
            VoucherRedemptionId = result.VoucherRedemptionId,
            CustomerVoucherId = result.CustomerVoucherId,
            VoucherDefinitionId = result.VoucherDefinitionId,
            VoucherDefinitionName = result.VoucherDefinitionName,
            VoucherPoolId = result.VoucherPoolId,
            VoucherCode = result.VoucherCode,
            CampaignId = result.CampaignId,
            CampaignName = result.CampaignName,
            CampaignSessionId = result.CampaignSessionId,
            ActionId = result.ActionId,
            ActionType = result.ActionType,
            SourceEventId = result.SourceEventId,
            RedeemedAt = result.RedeemedAt
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<CustomerPointTransactionResponseDto>> ToPagedResponseDto(
        this PagedResult<CustomerPointTransactionResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<CustomerPointTransactionResponseDto>>
        {
            Data = result.Items.Select(item => item.ToResponseDto()).ToArray(),
            Meta = result.ToMetaDto()
        };
    }

    public static CustomerPointTransactionResponseDto ToResponseDto(
        this CustomerPointTransactionResult result)
    {
        return new CustomerPointTransactionResponseDto
        {
            PointTransactionId = result.PointTransactionId,
            TransactionType = result.TransactionType,
            Amount = result.Amount,
            BalanceBefore = result.BalanceBefore,
            BalanceAfter = result.BalanceAfter,
            CampaignId = result.CampaignId,
            CampaignName = result.CampaignName,
            CampaignSessionId = result.CampaignSessionId,
            ActionId = result.ActionId,
            ActionType = result.ActionType,
            SourceEventId = result.SourceEventId,
            CreatedAt = result.CreatedAt
        };
    }

    private static ApiMetaDto ToMetaDto<T>(this PagedResult<T> result)
    {
        return new ApiMetaDto
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }
}
