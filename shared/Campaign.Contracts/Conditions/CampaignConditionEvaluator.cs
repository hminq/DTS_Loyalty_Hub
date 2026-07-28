using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;

namespace Campaign.Contracts.Conditions;

public sealed class CampaignConditionEvaluator
{
    public bool Matches(
        CampaignCondition condition,
        CampaignEventDefinition eventDefinition,
        IReadOnlyDictionary<string, string> facts)
    {
        var fields = eventDefinition.ConditionFields.ToDictionary(
            field => field.Code,
            StringComparer.OrdinalIgnoreCase);

        foreach (var predicate in condition.All)
        {
            if (!fields.ContainsKey(predicate.Field) ||
                !facts.TryGetValue(predicate.Field, out var rawFact) ||
                string.IsNullOrWhiteSpace(rawFact))
            {
                return false;
            }

            var fact = rawFact.ToUpperInvariant();
            if (predicate.Operator == CampaignConditionOperators.Equals &&
                fact != predicate.Values[0])
            {
                return false;
            }

            if (predicate.Operator == CampaignConditionOperators.In &&
                !predicate.Values.Contains(fact, StringComparer.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
