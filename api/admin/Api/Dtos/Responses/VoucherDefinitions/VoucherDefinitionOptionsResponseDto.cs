namespace Api.Dtos.Responses.VoucherDefinitions;

public sealed record VoucherDefinitionOptionsResponseDto(
    IReadOnlyCollection<VoucherDefinitionOptionResponseDto> RewardTypes,
    IReadOnlyCollection<VoucherDefinitionOptionResponseDto> ValidityTypes,
    IReadOnlyCollection<VoucherDefinitionOptionResponseDto> PublishTypes,
    IReadOnlyCollection<VoucherDefinitionOptionResponseDto> GenerationTypes);
