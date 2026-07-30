using System.Text.Json;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using Messaging.Contracts.Events;

namespace Campaign.Contracts.Conditions;

public sealed class CampaignConditionParser
{
    private static readonly JsonSerializerOptions CanonicalJsonOptions = new(JsonSerializerDefaults.Web);

    public CampaignConditionParseResult Parse(
        string conditionJson,
        CampaignEventDefinition eventDefinition)
    {
        if (string.IsNullOrWhiteSpace(conditionJson))
        {
            return Invalid("CAMPAIGN_CONDITION_REQUIRED", "Campaign condition is required.");
        }

        try
        {
            using var document = JsonDocument.Parse(conditionJson);
            return Parse(document.RootElement, eventDefinition);
        }
        catch (JsonException)
        {
            return Invalid("CAMPAIGN_CONDITION_INVALID_JSON", "Campaign condition must be valid JSON.");
        }
    }

    public CampaignConditionParseResult Parse(
        JsonElement conditionJson,
        CampaignEventDefinition eventDefinition)
    {
        var errors = new List<CampaignDefinitionError>();
        var fields = eventDefinition.ConditionFields.ToDictionary(
            field => field.Code,
            StringComparer.Ordinal);

        if (conditionJson.ValueKind != JsonValueKind.Object)
        {
            return Invalid("CAMPAIGN_CONDITION_INVALID_SHAPE", "Campaign condition must be a JSON object.");
        }

        var rootProperties = conditionJson.EnumerateObject().ToArray();
        if (rootProperties.Any(property => property.Name != "all"))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_UNKNOWN_PROPERTY",
                "Campaign condition only supports the all property."));
        }

        if (!conditionJson.TryGetProperty("all", out var allElement) ||
            allElement.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_ALL_INVALID",
                "Campaign condition all must be an array.",
                "all"));
        }

        if (errors.Count > 0)
        {
            return new CampaignConditionParseResult(null, null, errors);
        }

        var predicates = new List<CampaignConditionPredicate>();
        foreach (var predicateElement in allElement.EnumerateArray())
        {
            ParsePredicate(predicateElement, fields, predicates, errors);
        }

        if (errors.Count > 0)
        {
            return new CampaignConditionParseResult(null, null, errors);
        }

        var canonicalPredicates = predicates
            .OrderBy(predicate => predicate.Field, StringComparer.Ordinal)
            .ThenBy(predicate => predicate.Operator, StringComparer.Ordinal)
            .ThenBy(predicate => string.Join('\u001f', predicate.Values), StringComparer.Ordinal)
            .ToArray();

        var duplicate = canonicalPredicates
            .GroupBy(BuildPredicateKey, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            return Invalid(
                "CAMPAIGN_CONDITION_DUPLICATE_PREDICATE",
                "Campaign condition contains duplicate predicates.");
        }

        var condition = new CampaignCondition(canonicalPredicates);
        var canonicalJson = ToCanonicalJson(condition);
        return new CampaignConditionParseResult(condition, canonicalJson, Array.Empty<CampaignDefinitionError>());
    }

    public static string ToCanonicalJson(CampaignCondition condition)
    {
        var payload = new
        {
            all = condition.All.Select(predicate => new
            {
                field = predicate.Field,
                @operator = predicate.Operator,
                value = predicate.Operator is not CampaignConditionOperators.In
                    ? ToJsonValue(predicate.Values[0], predicate.DataType)
                    : predicate.Values
            })
        };

        return JsonSerializer.Serialize(payload, CanonicalJsonOptions);
    }

    private static void ParsePredicate(
        JsonElement predicateElement,
        IReadOnlyDictionary<string, CampaignConditionFieldDefinition> fields,
        List<CampaignConditionPredicate> predicates,
        List<CampaignDefinitionError> errors)
    {
        if (predicateElement.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_PREDICATE_INVALID",
                "Campaign condition predicate must be an object."));
            return;
        }

        var propertyNames = predicateElement.EnumerateObject()
            .Select(property => property.Name)
            .ToArray();
        if (propertyNames.Any(name => name is not ("field" or "operator" or "value")) ||
            !propertyNames.Contains("field") ||
            !propertyNames.Contains("operator") ||
            !propertyNames.Contains("value"))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_PREDICATE_INVALID",
                "Campaign condition predicate must contain only field, operator, and value."));
            return;
        }

        var fieldCode = ReadString(predicateElement, "field")?.Trim();
        var operatorCode = ReadString(predicateElement, "operator")?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(fieldCode) ||
            string.IsNullOrWhiteSpace(operatorCode))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_PREDICATE_INVALID",
                "Campaign condition field and operator are required."));
            return;
        }

        if (!fields.TryGetValue(fieldCode, out var field))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_FIELD_UNKNOWN",
                $"Campaign condition field '{fieldCode}' is not registered."));
            return;
        }

        if (!field.Operators.Contains(operatorCode, StringComparer.Ordinal))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_OPERATOR_INVALID",
                $"Campaign condition operator '{operatorCode}' is not allowed for field '{field.Code}'."));
            return;
        }

        var values = ReadValues(predicateElement.GetProperty("value"), operatorCode, field, errors);
        if (values.Count == 0)
        {
            return;
        }

        var normalizedValues = values
            .Select(value => NormalizeValue(value, field.DataType))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        if (field.DataType == CampaignConditionFieldTypes.Enum)
        {
            var allowedValues = field.Options.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (normalizedValues.Any(value => !allowedValues.Contains(value)))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    $"Campaign condition contains an unregistered value for field '{field.Code}'."));
                return;
            }
        }

        if (normalizedValues.Length != normalizedValues.Distinct(StringComparer.Ordinal).Count())
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_VALUE_DUPLICATE",
                $"Campaign condition contains duplicate values for field '{field.Code}'."));
            return;
        }

        if (operatorCode is not CampaignConditionOperators.In && normalizedValues.Length != 1)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_VALUE_INVALID",
                "Predicate operator requires exactly one value."));
            return;
        }

        predicates.Add(new CampaignConditionPredicate(field.Code, operatorCode, normalizedValues, field.DataType));
    }

    private static IReadOnlyList<string> ReadValues(
        JsonElement valueElement,
        string operatorCode,
        CampaignConditionFieldDefinition field,
        List<CampaignDefinitionError> errors)
    {
        if (operatorCode == CampaignConditionOperators.In)
        {
            if (field.DataType != CampaignConditionFieldTypes.Enum ||
                valueElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    "IN requires a non-empty string array value."));
                return Array.Empty<string>();
            }

            var values = valueElement.EnumerateArray()
                .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
                .ToArray();

            if (values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    "IN requires a non-empty string array value."));
                return Array.Empty<string>();
            }

            return values!;
        }

        if (valueElement.ValueKind is JsonValueKind.Null or JsonValueKind.Array)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_CONDITION_VALUE_INVALID",
                "Campaign condition value type is invalid."));
            return Array.Empty<string>();
        }

        if (field.DataType is CampaignConditionFieldTypes.String or CampaignConditionFieldTypes.Enum)
        {
            var value = valueElement.ValueKind == JsonValueKind.String
                ? valueElement.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    "EQUALS requires one non-empty string value."));
                return Array.Empty<string>();
            }

            if (field.Format == EventPayloadFormats.Uuid &&
                !Guid.TryParse(value, out _))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    "UUID condition values must be valid UUID strings."));
                return Array.Empty<string>();
            }

            return [value];
        }

        if (field.DataType == CampaignConditionFieldTypes.Number)
        {
            if (valueElement.ValueKind != JsonValueKind.Number ||
                !valueElement.TryGetDecimal(out var number))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    "NUMBER condition values must be decimal numbers."));
                return Array.Empty<string>();
            }

            return [number.ToString(System.Globalization.CultureInfo.InvariantCulture)];
        }

        if (field.DataType == CampaignConditionFieldTypes.Boolean)
        {
            if (valueElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_CONDITION_VALUE_INVALID",
                    "BOOLEAN condition values must be boolean."));
                return Array.Empty<string>();
            }

            return [valueElement.GetBoolean() ? "true" : "false"];
        }

        errors.Add(new CampaignDefinitionError(
            "CAMPAIGN_CONDITION_OPERATOR_INVALID",
            $"Campaign condition operator '{operatorCode}' is not supported."));
        return Array.Empty<string>();
    }

    private static string NormalizeValue(string value, string dataType)
    {
        return dataType == CampaignConditionFieldTypes.Enum
            ? value.Trim().ToUpperInvariant()
            : value.Trim();
    }

    private static object ToJsonValue(string value, string dataType)
    {
        return dataType switch
        {
            CampaignConditionFieldTypes.Number => decimal.Parse(
                value,
                System.Globalization.CultureInfo.InvariantCulture),
            CampaignConditionFieldTypes.Boolean => bool.Parse(value),
            _ => value
        };
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        var property = element.GetProperty(propertyName);
        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static string BuildPredicateKey(CampaignConditionPredicate predicate) =>
        $"{predicate.Field}:{predicate.Operator}:{string.Join(',', predicate.Values)}";

    private static CampaignConditionParseResult Invalid(string code, string message) =>
        new(null, null, [new CampaignDefinitionError(code, message)]);
}
