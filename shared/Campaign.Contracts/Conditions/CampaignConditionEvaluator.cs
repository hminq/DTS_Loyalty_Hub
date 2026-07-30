using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using System.Globalization;

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
            StringComparer.Ordinal);

        foreach (var predicate in condition.All)
        {
            if (!fields.TryGetValue(predicate.Field, out var field) ||
                !facts.TryGetValue(predicate.Field, out var rawFact) ||
                rawFact.Length == 0)
            {
                return false;
            }

            if (!MatchesPredicate(field.DataType, rawFact, predicate))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesPredicate(
        string dataType,
        string rawFact,
        CampaignConditionPredicate predicate)
    {
        return dataType switch
        {
            CampaignConditionFieldTypes.String => MatchesString(rawFact, predicate),
            CampaignConditionFieldTypes.Number => MatchesNumber(rawFact, predicate),
            CampaignConditionFieldTypes.Boolean => MatchesBoolean(rawFact, predicate),
            CampaignConditionFieldTypes.Enum => MatchesEnum(rawFact, predicate),
            _ => false
        };
    }

    private static bool MatchesString(
        string fact,
        CampaignConditionPredicate predicate)
    {
        return predicate.Operator switch
        {
            CampaignConditionOperators.Equals =>
                fact == predicate.Values[0],
            CampaignConditionOperators.NotEquals =>
                fact != predicate.Values[0],
            CampaignConditionOperators.Contains =>
                fact.Contains(predicate.Values[0], StringComparison.Ordinal),
            _ => false
        };
    }

    private static bool MatchesNumber(
        string rawFact,
        CampaignConditionPredicate predicate)
    {
        if (!decimal.TryParse(
                rawFact,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var fact) ||
            !decimal.TryParse(
                predicate.Values[0],
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var expected))
        {
            return false;
        }

        return predicate.Operator switch
        {
            CampaignConditionOperators.Equals => fact == expected,
            CampaignConditionOperators.NotEquals => fact != expected,
            CampaignConditionOperators.GreaterThan => fact > expected,
            CampaignConditionOperators.GreaterThanOrEquals => fact >= expected,
            CampaignConditionOperators.LessThan => fact < expected,
            CampaignConditionOperators.LessThanOrEquals => fact <= expected,
            _ => false
        };
    }

    private static bool MatchesBoolean(
        string rawFact,
        CampaignConditionPredicate predicate)
    {
        if (!bool.TryParse(rawFact, out var fact) ||
            !bool.TryParse(predicate.Values[0], out var expected))
        {
            return false;
        }

        return predicate.Operator switch
        {
            CampaignConditionOperators.Equals => fact == expected,
            CampaignConditionOperators.NotEquals => fact != expected,
            _ => false
        };
    }

    private static bool MatchesEnum(
        string rawFact,
        CampaignConditionPredicate predicate)
    {
        var fact = rawFact.ToUpperInvariant();
        return predicate.Operator switch
        {
            CampaignConditionOperators.Equals =>
                fact == predicate.Values[0],
            CampaignConditionOperators.In =>
                predicate.Values.Contains(fact, StringComparer.Ordinal),
            _ => false
        };
    }
}
