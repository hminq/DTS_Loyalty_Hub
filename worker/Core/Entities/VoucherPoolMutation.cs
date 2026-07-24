namespace Core.Entities;

public sealed record VoucherPoolMutation(
    Guid VoucherPoolId,
    Guid VoucherDefinitionId,
    string VoucherCode,
    string Status,
    DateTime CreatedAt);
