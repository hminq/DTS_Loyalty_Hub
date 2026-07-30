using System.Text.Json;
using Campaign.Contracts.Conditions;
using Campaign.Contracts.Constants;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Definitions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Services;

public sealed record GenericConditionEvaluationResult(
    bool IsValid,
    bool IsMatch,
    CampaignCondition? Condition = null);

public sealed class GenericCampaignConditionEvaluator
{
    public GenericConditionEvaluationResult Evaluate(
        string conditionJson,
        PublishedEventDefinition definition,
        IReadOnlyDictionary<string, ValidatedPayloadValue> payloadValues)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(payloadValues);

        if (string.IsNullOrWhiteSpace(conditionJson))
        {
            return Invalid();
        }

        try
        {
            using var doc = JsonDocument.Parse(conditionJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Invalid();
            }

            var properties = root.EnumerateObject().ToArray();
            if (properties.Length != 1 || properties[0].Name != "all")
            {
                return Invalid();
            }

            var allElement = properties[0].Value;
            if (allElement.ValueKind != JsonValueKind.Array)
            {
                return Invalid();
            }

            var predicates = new List<TypedPredicate>();
            var campaignPredicates = new List<CampaignConditionPredicate>();
            var seenPredicateKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var predicateElement in allElement.EnumerateArray())
            {
                if (predicateElement.ValueKind != JsonValueKind.Object)
                {
                    return Invalid();
                }

                var predProperties = predicateElement.EnumerateObject().ToArray();
                if (predProperties.Length != 3)
                {
                    return Invalid();
                }

                var fieldProp = predProperties.FirstOrDefault(p => p.Name == "field");
                var operatorProp = predProperties.FirstOrDefault(p => p.Name == "operator");
                var valueProp = predProperties.FirstOrDefault(p => p.Name == "value");

                if (fieldProp.Name == null || operatorProp.Name == null || valueProp.Name == null)
                {
                    return Invalid();
                }

                if (fieldProp.Value.ValueKind != JsonValueKind.String ||
                    operatorProp.Value.ValueKind != JsonValueKind.String)
                {
                    return Invalid();
                }

                var fieldCode = fieldProp.Value.GetString();
                var operatorCode = operatorProp.Value.GetString();

                if (string.IsNullOrEmpty(fieldCode) || string.IsNullOrEmpty(operatorCode))
                {
                    return Invalid();
                }

                if (!definition.FieldsByCode.TryGetValue(fieldCode, out var fieldSchema) ||
                    !fieldSchema.Conditionable)
                {
                    return Invalid();
                }

                if (!IsOperatorAllowed(fieldSchema.Type, operatorCode))
                {
                    return Invalid();
                }

                var (isValidValue, parsedValue, valueString) = ValidateAndParseValue(fieldSchema, valueProp.Value);
                if (!isValidValue)
                {
                    return Invalid();
                }

                var predicateKey = $"{fieldCode}:{operatorCode}:{valueString}";
                if (!seenPredicateKeys.Add(predicateKey))
                {
                    return Invalid();
                }

                predicates.Add(new TypedPredicate(fieldSchema, operatorCode, parsedValue!));
                campaignPredicates.Add(new CampaignConditionPredicate(
                    fieldSchema.Code,
                    operatorCode,
                    [valueString!],
                    fieldSchema.Type));
            }

            var condition = new CampaignCondition(campaignPredicates);

            foreach (var pred in predicates)
            {
                if (!payloadValues.TryGetValue(pred.FieldSchema.Code, out var payloadValue))
                {
                    return pred.FieldSchema.Required
                        ? Invalid()
                        : new GenericConditionEvaluationResult(true, false, condition);
                }

                if (!EvaluatePredicate(pred, payloadValue))
                {
                    return new GenericConditionEvaluationResult(true, false, condition);
                }
            }

            return new GenericConditionEvaluationResult(true, true, condition);
        }
        catch (JsonException)
        {
            return Invalid();
        }
    }

    private static bool IsOperatorAllowed(string fieldType, string operatorCode)
    {
        return fieldType switch
        {
            EventPayloadDataTypes.String => operatorCode is
                ConditionOperatorCodes.Equal or
                ConditionOperatorCodes.NotEquals or
                ConditionOperatorCodes.Contains,
            EventPayloadDataTypes.Number => operatorCode is
                ConditionOperatorCodes.Equal or
                ConditionOperatorCodes.NotEquals or
                ConditionOperatorCodes.GreaterThan or
                ConditionOperatorCodes.GreaterThanOrEquals or
                ConditionOperatorCodes.LessThan or
                ConditionOperatorCodes.LessThanOrEquals,
            EventPayloadDataTypes.Boolean => operatorCode is
                ConditionOperatorCodes.Equal or
                ConditionOperatorCodes.NotEquals,
            _ => false
        };
    }

    private static (bool IsValid, object? ParsedValue, string? ValueString) ValidateAndParseValue(
        EventPayloadFieldSchema fieldSchema,
        JsonElement valueElement)
    {
        if (valueElement.ValueKind is JsonValueKind.Null or JsonValueKind.Array or JsonValueKind.Object)
        {
            return (false, null, null);
        }

        switch (fieldSchema.Type)
        {
            case EventPayloadDataTypes.String:
                if (valueElement.ValueKind != JsonValueKind.String)
                {
                    return (false, null, null);
                }

                var str = valueElement.GetString() ?? string.Empty;
                if (fieldSchema.Format == EventPayloadFormats.Uuid)
                {
                    if (!Guid.TryParse(str, out var guid) || guid == Guid.Empty)
                    {
                        return (false, null, null);
                    }

                    var canonicalUuid = guid.ToString("D");
                    return (true, canonicalUuid, canonicalUuid);
                }

                return (true, str, str);

            case EventPayloadDataTypes.Number:
                if (valueElement.ValueKind != JsonValueKind.Number ||
                    !valueElement.TryGetDecimal(out var dec))
                {
                    return (false, null, null);
                }

                return (true, dec, dec.ToString(System.Globalization.CultureInfo.InvariantCulture));

            case EventPayloadDataTypes.Boolean:
                if (valueElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                {
                    return (false, null, null);
                }

                var b = valueElement.GetBoolean();
                return (true, b, b ? "true" : "false");

            default:
                return (false, null, null);
        }
    }

    private static bool EvaluatePredicate(TypedPredicate pred, ValidatedPayloadValue payloadValue)
    {
        switch (pred.FieldSchema.Type)
        {
            case EventPayloadDataTypes.String:
                var factStr = (string)payloadValue.Value;
                var condStr = (string)pred.ParsedValue;
                return pred.OperatorCode switch
                {
                    ConditionOperatorCodes.Equal => string.Equals(factStr, condStr, StringComparison.Ordinal),
                    ConditionOperatorCodes.NotEquals => !string.Equals(factStr, condStr, StringComparison.Ordinal),
                    ConditionOperatorCodes.Contains => factStr.Contains(condStr, StringComparison.Ordinal),
                    _ => false
                };

            case EventPayloadDataTypes.Number:
                var factDec = (decimal)payloadValue.Value;
                var condDec = (decimal)pred.ParsedValue;
                return pred.OperatorCode switch
                {
                    ConditionOperatorCodes.Equal => factDec == condDec,
                    ConditionOperatorCodes.NotEquals => factDec != condDec,
                    ConditionOperatorCodes.GreaterThan => factDec > condDec,
                    ConditionOperatorCodes.GreaterThanOrEquals => factDec >= condDec,
                    ConditionOperatorCodes.LessThan => factDec < condDec,
                    ConditionOperatorCodes.LessThanOrEquals => factDec <= condDec,
                    _ => false
                };

            case EventPayloadDataTypes.Boolean:
                var factBool = (bool)payloadValue.Value;
                var condBool = (bool)pred.ParsedValue;
                return pred.OperatorCode switch
                {
                    ConditionOperatorCodes.Equal => factBool == condBool,
                    ConditionOperatorCodes.NotEquals => factBool != condBool,
                    _ => false
                };

            default:
                return false;
        }
    }

    private static GenericConditionEvaluationResult Invalid() =>
        new(false, false, null);

    private sealed record TypedPredicate(
        EventPayloadFieldSchema FieldSchema,
        string OperatorCode,
        object ParsedValue);
}
