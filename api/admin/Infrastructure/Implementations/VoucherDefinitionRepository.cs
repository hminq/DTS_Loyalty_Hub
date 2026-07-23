using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Results;
using Microsoft.EntityFrameworkCore;
using DomainVoucherDefinition = Core.Entities.VoucherDefinition;
using PersistenceVoucherDefinition = Persistence.Models.VoucherDefinition;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class VoucherDefinitionRepository : IVoucherDefinitionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public VoucherDefinitionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim();

        return _dbContext.VoucherDefinitions
            .AsNoTracking()
            .AnyAsync(voucherDefinition => voucherDefinition.Code == normalizedCode, ct);
    }

    public Task<VoucherDefinitionResult?> GetByIdAsync(
        Guid voucherDefinitionId,
        CancellationToken ct = default)
    {
        return _dbContext.VoucherDefinitions
            .AsNoTracking()
            .Where(voucherDefinition =>
                voucherDefinition.VoucherDefinitionId == voucherDefinitionId)
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
                voucherDefinition.RemainingStock,
                voucherDefinition.CreatedAt,
                voucherDefinition.DeletedAt))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<PagedResult<VoucherDefinitionListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? rewardType,
        string? validityType,
        string? publishType,
        CancellationToken ct = default)
    {
        var query = _dbContext.VoucherDefinitions
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(voucherDefinition =>
                EF.Functions.ILike(voucherDefinition.Name, pattern) ||
                voucherDefinition.Code != null && EF.Functions.ILike(voucherDefinition.Code, pattern));
        }

        if (rewardType != null)
        {
            query = query.Where(voucherDefinition => voucherDefinition.RewardType == rewardType);
        }

        if (validityType != null)
        {
            query = query.Where(voucherDefinition => voucherDefinition.ValidityType == validityType);
        }

        if (publishType != null)
        {
            query = query.Where(voucherDefinition => voucherDefinition.PublishType == publishType);
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(voucherDefinition => voucherDefinition.CreatedAt)
            .ThenBy(voucherDefinition => voucherDefinition.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(voucherDefinition => new VoucherDefinitionListItemResult(
                voucherDefinition.VoucherDefinitionId,
                voucherDefinition.Code,
                voucherDefinition.Name,
                voucherDefinition.RewardType,
                voucherDefinition.RewardValue,
                voucherDefinition.PublishType,
                voucherDefinition.TotalStock,
                voucherDefinition.RemainingStock,
                voucherDefinition.CreatedAt,
                voucherDefinition.DeletedAt))
            .ToArrayAsync(ct);

        return new PagedResult<VoucherDefinitionListItemResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public DomainVoucherDefinition Add(DomainVoucherDefinition voucherDefinition)
    {
        _dbContext.VoucherDefinitions.Add(new PersistenceVoucherDefinition
        {
            VoucherDefinitionId = voucherDefinition.VoucherDefinitionId,
            Code = voucherDefinition.Code,
            Name = voucherDefinition.Name,
            Description = voucherDefinition.Description,
            BannerImageUrl = voucherDefinition.BannerImageUrl,
            RewardType = voucherDefinition.RewardType,
            RewardValue = voucherDefinition.RewardValue,
            ValidityType = voucherDefinition.ValidityType,
            ValidFrom = voucherDefinition.ValidFrom,
            ValidTo = voucherDefinition.ValidTo,
            DurationDay = voucherDefinition.DurationDay,
            GenerationType = voucherDefinition.GenerationType,
            PublishType = voucherDefinition.PublishType,
            TotalStock = voucherDefinition.TotalStock,
            RemainingStock = voucherDefinition.RemainingStock,
            CreatedAt = voucherDefinition.CreatedAt,
            DeletedAt = voucherDefinition.DeletedAt
        });

        return voucherDefinition;
    }
}
