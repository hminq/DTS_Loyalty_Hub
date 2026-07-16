using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Results;

namespace Core.Abstractions;

public interface IVoucherDefinitionRepository
{
    Task<PagedResult<VoucherDefinitionResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        CancellationToken ct = default);
}
