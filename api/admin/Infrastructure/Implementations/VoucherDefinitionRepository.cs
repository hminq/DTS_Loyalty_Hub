using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class VoucherDefinitionRepository : IVoucherDefinitionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public VoucherDefinitionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<VoucherDefinitionResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        CancellationToken ct = default)
    {
        var query = _dbContext.VoucherDefinitions
            .AsNoTracking()
            .Where(voucherDefinition => voucherDefinition.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(voucherDefinition =>
                EF.Functions.ILike(voucherDefinition.Name, pattern) ||
                voucherDefinition.Code != null && EF.Functions.ILike(voucherDefinition.Code, pattern));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(voucherDefinition => voucherDefinition.CreatedAt)
            .ThenBy(voucherDefinition => voucherDefinition.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(voucherDefinition => new VoucherDefinitionResult(
                voucherDefinition.VoucherDefinitionId,
                voucherDefinition.Code,
                voucherDefinition.Name,
                voucherDefinition.Description,
                voucherDefinition.BannerImageUrl,
                voucherDefinition.RewardType,
                voucherDefinition.RewardValue,
                voucherDefinition.ValidityType,
                voucherDefinition.ValidFrom,
                voucherDefinition.ValidTo,
                voucherDefinition.DurationDay,
                voucherDefinition.GenerationType,
                voucherDefinition.PublishType,
                voucherDefinition.TotalStock,
                voucherDefinition.CreatedAt,
                voucherDefinition.DeletedAt))
            .ToArrayAsync(ct);

        return new PagedResult<VoucherDefinitionResult>(
            items,
            page,
            pageSize,
            totalItems);
    }
}
