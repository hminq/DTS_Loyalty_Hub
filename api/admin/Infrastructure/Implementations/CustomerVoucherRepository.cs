using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class CustomerVoucherRepository : ICustomerVoucherRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CustomerVoucherRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CustomerVoucherResult>> GetPagedVouchersAsync(
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        string? userKeyword,
        DateTime currentTime,
        CancellationToken ct = default)
    {
        var query = _dbContext.CustomerVouchers
            .AsNoTracking()
            .Where(voucher => voucher.VoucherDef.DeletedAt == null);

        if (voucherKeyword is not null)
        {
            var pattern = $"%{voucherKeyword}%";
            query = query.Where(voucher =>
                EF.Functions.ILike(voucher.VoucherDef.Name, pattern));
        }

        if (rewardType is not null)
        {
            query = query.Where(voucher => voucher.VoucherDef.RewardType == rewardType);
        }

        if (redeemAtFrom.HasValue)
        {
            query = query.Where(voucher => voucher.RedeemedAt >= redeemAtFrom.Value);
        }

        if (redeemAtTo.HasValue)
        {
            query = query.Where(voucher => voucher.RedeemedAt <= redeemAtTo.Value);
        }

        if (userKeyword is not null)
        {
            var pattern = $"%{userKeyword}%";
            query = query.Where(voucher =>
                EF.Functions.ILike(voucher.Customer.User.Username, pattern) ||
                EF.Functions.ILike(voucher.Customer.User.Email, pattern) ||
                voucher.Customer.User.PhoneNumber != null &&
                EF.Functions.ILike(voucher.Customer.User.PhoneNumber, pattern));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(voucher => voucher.RedeemedAt)
            .ThenByDescending(voucher => voucher.CustomerVoucherId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(voucher => new CustomerVoucherResult(
                voucher.CustomerVoucherId,
                new CustomerInfoResult(
                    voucher.CustomerId,
                    voucher.Customer.User.Username,
                    voucher.Customer.User.Email,
                    voucher.Customer.User.PhoneNumber),
                voucher.VoucherDef.Name,
                voucher.VoucherDef.RewardType,
                voucher.ValidTo < currentTime))
            .ToArrayAsync(ct);

        return new PagedResult<CustomerVoucherResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public Task<CustomerVoucherDetailResult?> GetVoucherDetailAsync(
        Guid customerVoucherId,
        CancellationToken ct = default)
    {
        return _dbContext.CustomerVouchers
            .AsNoTracking()
            .Where(voucher =>
                voucher.CustomerVoucherId == customerVoucherId &&
                voucher.VoucherDef.DeletedAt == null)
            .Select(voucher => new CustomerVoucherDetailResult(
                voucher.CustomerVoucherId,
                new CustomerInfoResult(
                    voucher.CustomerId,
                    voucher.Customer.User.Username,
                    voucher.Customer.User.Email,
                    voucher.Customer.User.PhoneNumber),
                voucher.VoucherDefId,
                voucher.VoucherDef.Name,
                voucher.VoucherDef.Description,
                voucher.VoucherDef.RewardType,
                voucher.VoucherDef.BannerImageUrl,
                voucher.VoucherDef.GenerationType,
                voucher.ValidFrom,
                voucher.ValidTo,
                voucher.RedeemedAt))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<PagedResult<CustomerRedeemResult>> GetPagedRedeemsAsync(
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        string? campaignName,
        string? userKeyword,
        CancellationToken ct = default)
    {
        var query = _dbContext.VoucherRedemptions
            .AsNoTracking()
            .Where(redemption => redemption.VoucherDef.DeletedAt == null);

        if (voucherKeyword is not null)
        {
            var pattern = $"%{voucherKeyword}%";
            query = query.Where(redemption =>
                EF.Functions.ILike(redemption.VoucherDef.Name, pattern));
        }

        if (rewardType is not null)
        {
            query = query.Where(redemption => redemption.VoucherDef.RewardType == rewardType);
        }

        if (redeemAtFrom.HasValue)
        {
            query = query.Where(redemption => redemption.RedeemedAt >= redeemAtFrom.Value);
        }

        if (redeemAtTo.HasValue)
        {
            query = query.Where(redemption => redemption.RedeemedAt <= redeemAtTo.Value);
        }

        if (campaignName is not null)
        {
            var pattern = $"%{campaignName}%";
            query = query.Where(redemption =>
                redemption.Campaign != null &&
                EF.Functions.ILike(redemption.Campaign.CampaignName, pattern));
        }

        if (userKeyword is not null)
        {
            var pattern = $"%{userKeyword}%";
            query = query.Where(redemption =>
                EF.Functions.ILike(redemption.Customer.User.Username, pattern) ||
                EF.Functions.ILike(redemption.Customer.User.Email, pattern) ||
                redemption.Customer.User.PhoneNumber != null &&
                EF.Functions.ILike(redemption.Customer.User.PhoneNumber, pattern));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(redemption => redemption.RedeemedAt)
            .ThenByDescending(redemption => redemption.VoucherRedemptionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(redemption => new CustomerRedeemResult(
                redemption.VoucherRedemptionId,
                new CustomerInfoResult(
                    redemption.CustomerId,
                    redemption.Customer.User.Username,
                    redemption.Customer.User.Email,
                    redemption.Customer.User.PhoneNumber),
                redemption.CustomerVoucherId,
                redemption.VoucherDefId,
                redemption.VoucherDef.Name,
                redemption.VoucherDef.Description,
                redemption.CampaignId,
                redemption.Campaign == null ? null : redemption.Campaign.CampaignName,
                redemption.CustomerVoucher.ValidFrom,
                redemption.CustomerVoucher.ValidTo,
                redemption.RedeemedAt))
            .ToArrayAsync(ct);

        return new PagedResult<CustomerRedeemResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public Task<CustomerRedeemDetailResult?> GetRedeemDetailAsync(
        Guid voucherRedemptionId,
        CancellationToken ct = default)
    {
        return _dbContext.VoucherRedemptions
            .AsNoTracking()
            .Where(redemption =>
                redemption.VoucherRedemptionId == voucherRedemptionId &&
                redemption.VoucherDef.DeletedAt == null)
            .Select(redemption => new CustomerRedeemDetailResult(
                redemption.VoucherRedemptionId,
                redemption.RedeemedAt,
                new CustomerRedeemCustomerResult(
                    redemption.CustomerId,
                    redemption.Customer.User.Username,
                    redemption.Customer.User.Email,
                    redemption.Customer.User.PhoneNumber),
                new CustomerRedeemVoucherResult(
                    redemption.CustomerVoucherId,
                    redemption.VoucherDefId,
                    redemption.VoucherPoolId,
                    redemption.VoucherDef.Name,
                    redemption.VoucherDef.Description,
                    redemption.VoucherDef.BannerImageUrl,
                    redemption.VoucherCode,
                    redemption.VoucherDef.RewardType,
                    redemption.VoucherDef.RewardValue,
                    redemption.VoucherDef.GenerationType,
                    redemption.CustomerVoucher.ValidFrom,
                    redemption.CustomerVoucher.ValidTo),
                new CustomerRedeemIssuanceSourceResult(
                    redemption.CampaignId,
                    redemption.Campaign == null ? null : redemption.Campaign.CampaignName,
                    redemption.Campaign == null ? null : redemption.Campaign.EventType,
                    redemption.CampaignSessionId,
                    redemption.CampaignSession == null ? null : redemption.CampaignSession.SessionStart,
                    redemption.CampaignSession == null ? null : redemption.CampaignSession.SessionEnd,
                    redemption.CampaignSession == null ? null : redemption.CampaignSession.Status,
                    redemption.ActionId,
                    redemption.Action == null ? null : redemption.Action.ActionType)))
            .SingleOrDefaultAsync(ct);
    }
}
