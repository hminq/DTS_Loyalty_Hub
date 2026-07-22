using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class CustomerUserRepository : ICustomerUserRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CustomerUserRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CustomerUserListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        Guid? tierId,
        CancellationToken ct = default)
    {
        var query = _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.User.UserType == UserTypes.Customer);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(customer =>
                EF.Functions.ILike(customer.User.Username, pattern) ||
                EF.Functions.ILike(customer.User.Email, pattern) ||
                (customer.User.FullName != null &&
                    EF.Functions.ILike(customer.User.FullName, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToUpperInvariant();
            query = query.Where(customer => customer.User.Status == normalizedStatus);
        }

        if (tierId.HasValue)
        {
            query = query.Where(customer => customer.TierId == tierId.Value);
        }

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(customer => customer.CreatedAt)
            .ThenByDescending(customer => customer.CustomerId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(customer => new CustomerUserListItemResult(
                customer.CustomerId,
                customer.UserId,
                customer.User.Username,
                customer.User.Email,
                customer.User.FullName ?? string.Empty,
                customer.User.PhoneNumber,
                customer.TierId,
                customer.Tier != null ? customer.Tier.Name : null,
                customer.User.Status,
                customer.CreatedAt))
            .ToArrayAsync(ct);

        return new PagedResult<CustomerUserListItemResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public Task<CustomerUserDetailResult?> GetByIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .Where(customer =>
                customer.CustomerId == customerId &&
                customer.User.UserType == UserTypes.Customer)
            .Select(customer => new CustomerUserDetailResult(
                customer.CustomerId,
                customer.UserId,
                customer.User.Username,
                customer.User.Email,
                customer.User.FullName ?? string.Empty,
                customer.User.PhoneNumber,
                customer.User.Status,
                customer.CreatedAt,
                customer.CurrentTierPoint,
                customer.NextTierPoint,
                customer.Tier == null
                    ? null
                    : new CustomerUserTierResult(
                        customer.Tier.TierConfigId,
                        customer.Tier.Name,
                        customer.Tier.PointsRequired,
                        customer.Tier.CycleMonth,
                        customer.Tier.Priority)))
            .SingleOrDefaultAsync(ct);
    }

    public Task<CustomerUserPointsResult?> GetPointsAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .Where(customer =>
                customer.CustomerId == customerId &&
                customer.User.UserType == UserTypes.Customer)
            .Select(customer => new CustomerUserPointsResult(
                customer.CustomerId,
                customer.CurrentTierPoint,
                customer.NextTierPoint,
                customer.Tier == null
                    ? null
                    : new CustomerUserTierResult(
                        customer.Tier.TierConfigId,
                        customer.Tier.Name,
                        customer.Tier.PointsRequired,
                        customer.Tier.CycleMonth,
                        customer.Tier.Priority),
                customer.CustomerPoint == null ? 0 : customer.CustomerPoint.ActivePoint,
                customer.CustomerPoint == null ? 0 : customer.CustomerPoint.LockedPoint,
                customer.CustomerPoint == null ? 0 : customer.CustomerPoint.LifetimePoint,
                customer.CustomerPoint == null ? 0 : customer.CustomerPoint.SpentPoint,
                customer.CustomerPoint == null ? 0 : customer.CustomerPoint.ExpiredPoint,
                customer.CustomerPoint == null ? null : customer.CustomerPoint.UpdatedAt))
            .SingleOrDefaultAsync(ct);
    }

    public Task<bool> ExistsAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(
                customer =>
                    customer.CustomerId == customerId &&
                    customer.User.UserType == UserTypes.Customer,
                ct);
    }

    public async Task<PagedResult<CustomerVoucherResult>> GetVouchersAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.CustomerVouchers
            .AsNoTracking()
            .Where(customerVoucher => customerVoucher.CustomerId == customerId);

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(customerVoucher => customerVoucher.RedeemedAt)
            .ThenByDescending(customerVoucher => customerVoucher.CustomerVoucherId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(customerVoucher => new CustomerVoucherResult(
                customerVoucher.CustomerVoucherId,
                customerVoucher.VoucherDefId,
                customerVoucher.VoucherDef.Name,
                customerVoucher.VoucherPoolId,
                customerVoucher.VoucherCode,
                customerVoucher.ValidFrom,
                customerVoucher.ValidTo,
                customerVoucher.RemainingCount,
                customerVoucher.RedeemedAt))
            .ToArrayAsync(ct);

        return new PagedResult<CustomerVoucherResult>(items, page, pageSize, totalItems);
    }

    public async Task<PagedResult<CustomerVoucherRedemptionResult>> GetVoucherRedemptionsAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.VoucherRedemptions
            .AsNoTracking()
            .Where(redemption => redemption.CustomerId == customerId);

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(redemption => redemption.RedeemedAt)
            .ThenByDescending(redemption => redemption.VoucherRedemptionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(redemption => new CustomerVoucherRedemptionResult(
                redemption.VoucherRedemptionId,
                redemption.CustomerVoucherId,
                redemption.VoucherDefId,
                redemption.VoucherDef.Name,
                redemption.VoucherPoolId,
                redemption.VoucherCode,
                redemption.CampaignId,
                redemption.Campaign != null ? redemption.Campaign.CampaignName : null,
                redemption.CampaignSessionId,
                redemption.ActionId,
                redemption.Action != null ? redemption.Action.ActionType : null,
                redemption.SourceEventId,
                redemption.RedeemedAt))
            .ToArrayAsync(ct);

        return new PagedResult<CustomerVoucherRedemptionResult>(items, page, pageSize, totalItems);
    }

    public async Task<PagedResult<CustomerPointTransactionResult>> GetPointTransactionsAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.PointTransactions
            .AsNoTracking()
            .Where(transaction => transaction.CustomerId == customerId);

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ThenByDescending(transaction => transaction.PointTransactionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(transaction => new CustomerPointTransactionResult(
                transaction.PointTransactionId,
                transaction.TransactionType,
                transaction.Amount,
                transaction.BalanceBefore,
                transaction.BalanceAfter,
                transaction.CampaignId,
                transaction.Campaign != null ? transaction.Campaign.CampaignName : null,
                transaction.CampaignSessionId,
                transaction.ActionId,
                transaction.Action != null ? transaction.Action.ActionType : null,
                transaction.SourceEventId,
                transaction.CreatedAt))
            .ToArrayAsync(ct);

        return new PagedResult<CustomerPointTransactionResult>(items, page, pageSize, totalItems);
    }
}
