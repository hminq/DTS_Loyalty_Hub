namespace Campaign.Contracts.Definitions;

public sealed record CampaignValidationResult(IReadOnlyList<CampaignDefinitionError> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static CampaignValidationResult Success { get; } = new(Array.Empty<CampaignDefinitionError>());

    public static CampaignValidationResult Failure(params CampaignDefinitionError[] errors) => new(errors);
}
