using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;

namespace Campaign.Contracts.Conditions;

public sealed class CampaignConditionCompatibilityAnalyzer
{
    public bool CanOverlap(
        CampaignCondition first,
        CampaignCondition second,
        CampaignEventDefinition eventDefinition)
    {
        var fields = eventDefinition.ConditionFields
            .Where(field => field.DataType == CampaignConditionFieldTypes.Enum)
            .ToDictionary(field => field.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields.Values)
        {
            var firstAllowed = GetAllowedValues(first, field);
            var secondAllowed = GetAllowedValues(second, field);

            if (!firstAllowed.Overlaps(secondAllowed))
            {
                return false;
            }
        }

        return true;
    }

    private static HashSet<string> GetAllowedValues(
        CampaignCondition condition,
        CampaignConditionFieldDefinition field)
    {
        var allowed = field.Options
            .Select(option => option.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);

        foreach (var predicate in condition.All.Where(predicate =>
                     string.Equals(predicate.Field, field.Code, StringComparison.OrdinalIgnoreCase)))
        {
            var predicateValues = predicate.Values.ToHashSet(StringComparer.Ordinal);
            allowed.IntersectWith(predicateValues);
        }

        return allowed;
    }
}
