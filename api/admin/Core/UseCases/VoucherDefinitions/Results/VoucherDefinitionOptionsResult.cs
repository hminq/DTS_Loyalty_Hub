namespace Core.UseCases.VoucherDefinitions.Results;

public sealed record VoucherDefinitionOptionsResult(
    IReadOnlyCollection<string> RewardTypes,
    IReadOnlyCollection<string> ValidityTypes,
    IReadOnlyCollection<string> PublishTypes,
    IReadOnlyCollection<string> GenerationTypes);
