using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Conditions;
using Campaign.Contracts.Definitions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Definitions;
using Consumer.Core.Entities.Envelope;
using Consumer.Core.Exceptions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Services;

public sealed class GenericCampaignEventFactory
{
    public GenericValidatedCampaignEvent Create(
        RawEventEnvelope envelope,
        PublishedEventDefinition definition,
        string deliveryRoutingKey)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!string.Equals(envelope.EventType, definition.EventTypeCode, StringComparison.Ordinal) ||
            envelope.EventVersion != definition.Version)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventTypeUnsupported);
        }

        if (!string.Equals(deliveryRoutingKey, definition.RoutingKey, StringComparison.Ordinal))
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventRoutingKeyMismatch);
        }

        var values = ValidatePayload(envelope.PayloadElement, definition);
        var normalizedEnvelope = WriteCanonicalEnvelope(envelope, definition, values);
        var payloadHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(normalizedEnvelope)))
            .ToLowerInvariant();

        return new GenericValidatedCampaignEvent(
            envelope.EventId,
            definition.EventTypeId,
            definition.EventTypeVersionId,
            definition.EventTypeCode,
            definition.Version,
            definition.RoutingKey,
            NormalizeUtc(envelope.OccurredAt),
            definition,
            values,
            normalizedEnvelope,
            payloadHash);
    }

    public CampaignEventDefinition ToCampaignDefinition(
        PublishedEventDefinition definition)
    {
        return new CampaignEventDefinition(
            definition.EventTypeCode,
            definition.PayloadSchema.Targets.FirstOrDefault()?.Selector ?? string.Empty,
            definition.PayloadSchema.Fields
                .Where(field => field.Conditionable)
                .Select(field => new CampaignConditionFieldDefinition(
                    field.Code,
                    field.Type,
                    GetOperators(field.Type),
                    Array.Empty<string>(),
                    field.Required,
                    field.Format,
                    field.Conditionable))
                .ToArray(),
            definition.PayloadSchema.Targets
                .Select(target => new CampaignTargetDefinition(
                    target.Selector,
                    target.Kind,
                    CampaignCondition.MatchAll))
                .ToArray());
    }

    public CampaignFactValue GetFact(
        GenericValidatedCampaignEvent campaignEvent,
        string fieldCode)
    {
        if (!campaignEvent.PayloadValues.TryGetValue(fieldCode, out var value))
        {
            return new CampaignFactValue(fieldCode, string.Empty);
        }

        return new CampaignFactValue(fieldCode, value.AsFactString());
    }

    public CampaignTargetResolution ResolveTarget(
        GenericValidatedCampaignEvent campaignEvent,
        string selector)
    {
        if (!campaignEvent.Definition.TargetsBySelector.TryGetValue(selector, out var target))
        {
            return new CampaignTargetResolution(
                CampaignTargetResolutionStatuses.Unsupported,
                string.Empty,
                null,
                CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
        }

        if (!campaignEvent.PayloadValues.TryGetValue(target.IdField, out var value) ||
            value.Value is not string rawId ||
            !Guid.TryParse(rawId, out var targetId) ||
            targetId == Guid.Empty)
        {
            return new CampaignTargetResolution(
                CampaignTargetResolutionStatuses.InvalidEvent,
                target.Kind,
                null,
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        return new CampaignTargetResolution(
            CampaignTargetResolutionStatuses.Resolved,
            target.Kind,
            targetId,
            null);
    }

    private static IReadOnlyDictionary<string, ValidatedPayloadValue> ValidatePayload(
        JsonElement payload,
        PublishedEventDefinition definition)
    {
        var values = new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in payload.EnumerateObject())
        {
            if (!seen.Add(property.Name))
            {
                throw new CampaignEventValidationException(
                    CampaignProcessingErrorCodes.EventPayloadInvalid);
            }

            if (!definition.FieldsByCode.TryGetValue(property.Name, out var field))
            {
                throw new CampaignEventValidationException(
                    CampaignProcessingErrorCodes.EventPayloadInvalid);
            }

            values[field.Code] = new ValidatedPayloadValue(
                field.Code,
                field.Type,
                field.Format,
                ReadValue(field, property.Value));
        }

        foreach (var field in definition.PayloadSchema.Fields)
        {
            if (field.Required && !values.ContainsKey(field.Code))
            {
                throw new CampaignEventValidationException(
                    CampaignProcessingErrorCodes.EventPayloadInvalid);
            }
        }

        return values;
    }

    private static object ReadValue(
        EventPayloadFieldSchema field,
        JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Object or JsonValueKind.Array)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        return field.Type switch
        {
            EventPayloadDataTypes.String => ReadStringValue(field, value),
            EventPayloadDataTypes.Number => ReadDecimalValue(value),
            EventPayloadDataTypes.Boolean => ReadBooleanValue(value),
            _ => throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventPayloadInvalid)
        };
    }

    private static string ReadStringValue(
        EventPayloadFieldSchema field,
        JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        var text = value.GetString() ?? string.Empty;
        if (field.Format is null)
        {
            return text;
        }

        if (field.Format != EventPayloadFormats.Uuid ||
            !Guid.TryParse(text, out var parsed) ||
            parsed == Guid.Empty)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        return parsed.ToString("D");
    }

    private static decimal ReadDecimalValue(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Number ||
            !value.TryGetDecimal(out var result))
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        return result;
    }

    private static bool ReadBooleanValue(JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.True)
        {
            return true;
        }

        if (value.ValueKind is JsonValueKind.False)
        {
            return false;
        }

        throw new CampaignEventValidationException(
            CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    private static string WriteCanonicalEnvelope(
        RawEventEnvelope envelope,
        PublishedEventDefinition definition,
        IReadOnlyDictionary<string, ValidatedPayloadValue> values)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(
                   stream,
                   new JsonWriterOptions { Indented = false }))
        {
            writer.WriteStartObject();
            writer.WriteString("eventId", envelope.EventId.ToString("D"));
            writer.WriteString("eventType", definition.EventTypeCode);
            writer.WriteNumber("eventVersion", definition.Version);
            writer.WriteString(
                "occurredAt",
                NormalizeUtc(envelope.OccurredAt).ToString(
                    "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                    CultureInfo.InvariantCulture));
            writer.WritePropertyName("payload");
            writer.WriteStartObject();
            foreach (var field in definition.PayloadSchema.Fields)
            {
                if (!values.TryGetValue(field.Code, out var value))
                {
                    continue;
                }

                writer.WritePropertyName(field.Code);
                switch (value.Value)
                {
                    case string stringValue:
                        writer.WriteStringValue(stringValue);
                        break;
                    case decimal decimalValue:
                        writer.WriteRawValue(
                            decimalValue.ToString(CultureInfo.InvariantCulture));
                        break;
                    case bool boolValue:
                        writer.WriteBooleanValue(boolValue);
                        break;
                }
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static IReadOnlyList<string> GetOperators(string fieldType)
    {
        return fieldType switch
        {
            EventPayloadDataTypes.String =>
            [
                ConditionOperatorCodes.Equal,
                ConditionOperatorCodes.NotEquals,
                ConditionOperatorCodes.Contains
            ],
            EventPayloadDataTypes.Number =>
            [
                ConditionOperatorCodes.Equal,
                ConditionOperatorCodes.NotEquals,
                ConditionOperatorCodes.GreaterThan,
                ConditionOperatorCodes.GreaterThanOrEquals,
                ConditionOperatorCodes.LessThan,
                ConditionOperatorCodes.LessThanOrEquals
            ],
            EventPayloadDataTypes.Boolean =>
            [
                ConditionOperatorCodes.Equal,
                ConditionOperatorCodes.NotEquals
            ],
            _ => Array.Empty<string>()
        };
    }
}
