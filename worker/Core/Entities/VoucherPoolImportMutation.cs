namespace Core.Entities;

public sealed record VoucherPoolImportMutation(
    Guid JobId,
    int RowNumber,
    Guid VoucherPoolId,
    string VoucherCode,
    DateTime CreatedAt);
