using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Core.Entities.Constants;
using Core.Exceptions;
using Messaging.Contracts.Events;

namespace Core.UseCases.EventDefinitions;

public sealed partial class EventDefinitionSchemaService
{
    private static readonly JsonSerializerOptions SchemaJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EventDefinitionOptionsResult GetOptions()
    {
        return new EventDefinitionOptionsResult(
            [
                new EventDefinitionFieldTypeOptionResult(
                    EventPayloadDataTypes.String,
                    [EventPayloadFormats.Uuid],
                    [
                        ConditionOperatorCodes.Equal,
                        ConditionOperatorCodes.NotEquals,
                        ConditionOperatorCodes.Contains
                    ]),
                new EventDefinitionFieldTypeOptionResult(
                    EventPayloadDataTypes.Number,
                    [],
                    [
                        ConditionOperatorCodes.Equal,
                        ConditionOperatorCodes.NotEquals,
                        ConditionOperatorCodes.GreaterThan,
                        ConditionOperatorCodes.GreaterThanOrEquals,
                        ConditionOperatorCodes.LessThan,
                        ConditionOperatorCodes.LessThanOrEquals
                    ]),
                new EventDefinitionFieldTypeOptionResult(
                    EventPayloadDataTypes.Boolean,
                    [],
                    [
                        ConditionOperatorCodes.Equal,
                        ConditionOperatorCodes.NotEquals
                    ])
            ],
            [
                new EventDefinitionTargetKindOptionResult(
                    EventTargetKinds.Customer,
                    EventPayloadDataTypes.String,
                    EventPayloadFormats.Uuid)
            ],
            EventDefinitionStatuses.All.ToArray(),
            EventDefinitionVersionStatuses.All.ToArray(),
            new EventDefinitionLimitsResult(
                EventDefinitionSchemaLimits.MaximumFields,
                EventDefinitionSchemaLimits.MaximumTargets,
                EventDefinitionSchemaLimits.MaximumSchemaBytes));
    }

    public string SerializeDraft(EventPayloadSchema schema)
    {
        ValidateStructural(schema);
        return SerializeCanonical(schema);
    }

    public string SerializeForPublish(EventPayloadSchema schema)
    {
        ValidateStructural(schema);

        if (schema.Fields.Count == 0)
        {
            throw new DomainException("EVENT_DEFINITION_PUBLISH_FIELDS_REQUIRED", DomainErrorType.Validation);
        }

        if (schema.Targets.Count == 0)
        {
            throw new DomainException("EVENT_DEFINITION_PUBLISH_TARGETS_REQUIRED", DomainErrorType.Validation);
        }

        var serialized = SerializeCanonical(schema);
        if (Encoding.UTF8.GetByteCount(serialized) > EventDefinitionSchemaLimits.MaximumSchemaBytes)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_TOO_LARGE", DomainErrorType.Validation);
        }

        _ = Deserialize(serialized);

        return serialized;
    }

    public EventPayloadSchema Deserialize(string payloadSchema)
    {
        try
        {
            return JsonSerializer.Deserialize<EventPayloadSchema>(payloadSchema, SchemaJsonOptions)
                ?? throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }
        catch (JsonException)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }
    }

    private static string SerializeCanonical(EventPayloadSchema schema)
    {
        return JsonSerializer.Serialize(schema, SchemaJsonOptions);
    }

    private static void ValidateStructural(EventPayloadSchema schema)
    {
        if (schema.Fields is null || schema.Targets is null)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (schema.Fields.Count > EventDefinitionSchemaLimits.MaximumFields)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (schema.Targets.Count > EventDefinitionSchemaLimits.MaximumTargets)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        var fieldCodes = new HashSet<string>(StringComparer.Ordinal);
        var fieldByCode = new Dictionary<string, EventPayloadFieldSchema>(StringComparer.Ordinal);
        foreach (var field in schema.Fields)
        {
            ValidateField(field);
            if (!fieldCodes.Add(field.Code))
            {
                throw new DomainException("EVENT_DEFINITION_FIELD_DUPLICATE", DomainErrorType.Validation);
            }

            fieldByCode[field.Code] = field;
        }

        var selectors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var target in schema.Targets)
        {
            ValidateTarget(target);
            if (!selectors.Add(target.Selector))
            {
                throw new DomainException("EVENT_DEFINITION_TARGET_DUPLICATE", DomainErrorType.Validation);
            }

            if (!fieldByCode.TryGetValue(target.IdField, out var idField))
            {
                throw new DomainException("EVENT_DEFINITION_TARGET_FIELD_NOT_FOUND", DomainErrorType.Validation);
            }

            if (!idField.Required ||
                idField.Type != EventPayloadDataTypes.String ||
                idField.Format != EventPayloadFormats.Uuid)
            {
                throw new DomainException("EVENT_DEFINITION_TARGET_FIELD_INVALID", DomainErrorType.Validation);
            }
        }
    }

    private static void ValidateField(EventPayloadFieldSchema field)
    {
        if (string.IsNullOrWhiteSpace(field.Code) ||
            !FieldCodeRegex().IsMatch(field.Code))
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (field.Type is not (
            EventPayloadDataTypes.String or
            EventPayloadDataTypes.Number or
            EventPayloadDataTypes.Boolean))
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (field.Type == EventPayloadDataTypes.String)
        {
            if (field.Format is not null && field.Format != EventPayloadFormats.Uuid)
            {
                throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
            }

            return;
        }

        if (field.Format is not null)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }
    }

    private static void ValidateTarget(EventTargetSchema target)
    {
        if (string.IsNullOrWhiteSpace(target.Selector) ||
            !TargetSelectorRegex().IsMatch(target.Selector))
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (target.Kind != EventTargetKinds.Customer ||
            string.IsNullOrWhiteSpace(target.IdField))
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }
    }

    [GeneratedRegex("^[a-z][A-Za-z0-9]*$", RegexOptions.CultureInvariant)]
    private static partial Regex FieldCodeRegex();

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex TargetSelectorRegex();
}

public sealed record EventDefinitionOptionsResult(
    IReadOnlyCollection<EventDefinitionFieldTypeOptionResult> FieldTypes,
    IReadOnlyCollection<EventDefinitionTargetKindOptionResult> TargetKinds,
    IReadOnlyCollection<string> EventTypeStatuses,
    IReadOnlyCollection<string> VersionStatuses,
    EventDefinitionLimitsResult Limits);

public sealed record EventDefinitionFieldTypeOptionResult(
    string Code,
    IReadOnlyCollection<string> Formats,
    IReadOnlyCollection<string> Operators);

public sealed record EventDefinitionTargetKindOptionResult(
    string Code,
    string IdentityFieldType,
    string IdentityFieldFormat);

public sealed record EventDefinitionLimitsResult(
    int MaximumFields,
    int MaximumTargets,
    int MaximumSchemaBytes);
