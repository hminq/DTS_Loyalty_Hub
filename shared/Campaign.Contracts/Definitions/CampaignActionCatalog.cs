using Campaign.Contracts.Constants;

namespace Campaign.Contracts.Definitions;

public sealed class CampaignActionCatalog
{
    private readonly IReadOnlyDictionary<string, CampaignActionDefinition> _actions;

    public CampaignActionCatalog(IReadOnlyList<CampaignActionDefinition> actions)
    {
        Actions = actions.ToArray();
        Validate(Actions);
        _actions = Actions.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<CampaignActionDefinition> Actions { get; }

    public static CampaignActionCatalog BuiltIn { get; } = new(
    [
        new CampaignActionDefinition(
            ActionTypes.IssuePoint,
            CampaignTargetKinds.Customer,
            [
                new CampaignParameterFieldDefinition(
                    "amount",
                    "DECIMAL",
                    Required: true,
                    MinimumExclusive: 0,
                    Maximum: 9999999999999999.99m,
                    Scale: 2)
            ])
    ]);

    public bool TryGetAction(string code, out CampaignActionDefinition actionDefinition) =>
        _actions.TryGetValue(code, out actionDefinition!);

    private static void Validate(IReadOnlyList<CampaignActionDefinition> actions)
    {
        if (actions.Any(action => string.IsNullOrWhiteSpace(action.Code)))
        {
            throw new InvalidOperationException("Campaign action catalog contains a blank code.");
        }

        var duplicate = actions
            .Select(action => action.Code.Trim())
            .GroupBy(code => code, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Campaign action catalog contains duplicate code '{duplicate.Key}'.");
        }
    }
}
