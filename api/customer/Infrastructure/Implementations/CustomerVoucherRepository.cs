using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.Vouchers.Queries.GetCustomerVouchers;
using Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;
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
        Guid customerId,
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        DateTime currentTime,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CustomerVouchers
            .AsNoTracking()
            .Where(voucher =>
                voucher.CustomerId == customerId &&
                voucher.VoucherDef.DeletedAt == null);

        if (voucherKeyword is not null)
        {
            query = query.Where(voucher =>
                EF.Functions.ILike(voucher.VoucherDef.Name, $"%{voucherKeyword}%"));
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

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(voucher => voucher.RedeemedAt)
            .ThenByDescending(voucher => voucher.CustomerVoucherId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(voucher => new CustomerVoucherResult(
                voucher.CustomerVoucherId,
                voucher.VoucherDef.Name,
                voucher.VoucherDef.RewardType,
                voucher.ValidTo < currentTime))
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerVoucherResult>(items, totalItems, page, pageSize);
    }

    public Task<CustomerVoucherDetailResult?> GetVoucherDetailAsync(
        Guid customerId,
        Guid customerVoucherId,
        CancellationToken cancellationToken)
    {
        return _dbContext.CustomerVouchers
            .AsNoTracking()
            .Where(voucher =>
                voucher.CustomerVoucherId == customerVoucherId &&
                voucher.CustomerId == customerId &&
                voucher.VoucherDef.DeletedAt == null)
            .Select(voucher => new CustomerVoucherDetailResult(
                voucher.CustomerVoucherId,
                voucher.VoucherCode,
                voucher.VoucherDefId,
                voucher.VoucherDef.Name,
                voucher.VoucherDef.Description,
                voucher.VoucherDef.RewardType,
                voucher.VoucherDef.BannerImageUrl,
                voucher.ValidFrom,
                voucher.ValidTo,
                voucher.RedeemedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<VoucherRedemptionResult>> GetPagedRedemptionsAsync(
        Guid customerId,
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        string? campaignName,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.VoucherRedemptions
            .AsNoTracking()
            .Where(redemption =>
                redemption.CustomerId == customerId &&
                redemption.VoucherDef.DeletedAt == null);

        if (voucherKeyword is not null)
        {
            query = query.Where(redemption =>
                EF.Functions.ILike(redemption.VoucherDef.Name, $"%{voucherKeyword}%"));
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
            query = query.Where(redemption =>
                redemption.Campaign != null &&
                EF.Functions.ILike(redemption.Campaign.CampaignName, $"%{campaignName}%"));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(redemption => redemption.RedeemedAt)
            .ThenByDescending(redemption => redemption.VoucherRedemptionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(redemption => new VoucherRedemptionResult(
                redemption.VoucherRedemptionId,
                redemption.CustomerVoucherId,
                redemption.VoucherDefId,
                redemption.VoucherDef.Name,
                redemption.VoucherDef.Description,
                redemption.CampaignId,
                redemption.Campaign == null ? null : redemption.Campaign.CampaignName,
                redemption.CustomerVoucher.ValidFrom,
                redemption.CustomerVoucher.ValidTo,
                redemption.RedeemedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<VoucherRedemptionResult>(items, totalItems, page, pageSize);
    }
}
