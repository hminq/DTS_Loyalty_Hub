namespace Api.Dtos.Responses.VoucherDefinitions;

public sealed record VoucherDefinitionOptionsResponseDto(
    IReadOnlyCollection<string> RewardTypes,
    IReadOnlyCollection<string> ValidityTypes,
    IReadOnlyCollection<string> PublishTypes,
    IReadOnlyCollection<string> GenerationTypes,
    VoucherDefinitionConstraintsResponseDto Constraints);

public sealed record VoucherDefinitionConstraintsResponseDto(
    int MaxTotalStock,
    int MaxImportedTotalStock);
