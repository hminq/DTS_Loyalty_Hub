namespace Core.Abstractions;

public interface IVoucherPoolImportObjectKeyPolicy
{
    bool IsValid(Guid voucherDefinitionId, string? objectKey);
}
