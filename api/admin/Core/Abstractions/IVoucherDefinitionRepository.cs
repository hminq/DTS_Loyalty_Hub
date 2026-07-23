using Core.Entities;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Results;

namespace Core.Abstractions;

public interface IVoucherDefinitionRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    Task<VoucherDefinitionResult?> GetByIdAsync(
        Guid voucherDefinitionId,
        CancellationToken ct = default);

    Task<PagedResult<VoucherDefinitionListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? rewardType,
        string? validityType,
        string? publishType,
        CancellationToken ct = default);

    VoucherDefinition Add(VoucherDefinition voucherDefinition);
}
