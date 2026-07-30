using Messaging.Contracts.Events;

namespace Consumer.Core.Entities.Definitions;

public sealed record PublishedEventDefinition
{
    public PublishedEventDefinition(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        string eventTypeCode,
        string routingKey,
        int version,
        string status,
        DateTime publishedAt,
        EventPayloadSchema payloadSchema)
    {
        EventTypeId = eventTypeId;
        EventTypeVersionId = eventTypeVersionId;
        EventTypeCode = eventTypeCode ?? throw new ArgumentNullException(nameof(eventTypeCode));
        RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        Version = version;
        Status = status ?? throw new ArgumentNullException(nameof(status));
        PublishedAt = publishedAt;
        PayloadSchema = payloadSchema ?? throw new ArgumentNullException(nameof(payloadSchema));

        ValidateSchema(payloadSchema);

        var fields = new Dictionary<string, EventPayloadFieldSchema>(StringComparer.Ordinal);
        foreach (var field in payloadSchema.Fields)
        {
            fields.Add(field.Code, field);
        }
        FieldsByCode = fields;

        var targets = new Dictionary<string, EventTargetSchema>(StringComparer.Ordinal);
        foreach (var target in payloadSchema.Targets)
        {
            targets.Add(target.Selector, target);
        }
        TargetsBySelector = targets;
    }

    public Guid EventTypeId { get; }
    public Guid EventTypeVersionId { get; }
    public string EventTypeCode { get; }
    public string RoutingKey { get; }
    public int Version { get; }
    public string Status { get; }
    public DateTime PublishedAt { get; }
    public EventPayloadSchema PayloadSchema { get; }
    public IReadOnlyDictionary<string, EventPayloadFieldSchema> FieldsByCode { get; }
    public IReadOnlyDictionary<string, EventTargetSchema> TargetsBySelector { get; }

    private static void ValidateSchema(EventPayloadSchema schema)
    {
        if (schema.Fields is null || schema.Targets is null)
        {
            throw new ArgumentException("Published event payload schema is incomplete.", nameof(schema));
        }

        var fields = new Dictionary<string, EventPayloadFieldSchema>(StringComparer.Ordinal);
        foreach (var field in schema.Fields)
        {
            ValidateField(field);
            if (!fields.TryAdd(field.Code, field))
            {
                throw new ArgumentException(
                    $"Published event payload schema contains duplicate field '{field.Code}'.",
                    nameof(schema));
            }
        }

        var selectors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var target in schema.Targets)
        {
            ValidateTarget(target);
            if (!selectors.Add(target.Selector))
            {
                throw new ArgumentException(
                    $"Published event payload schema contains duplicate target '{target.Selector}'.",
                    nameof(schema));
            }

            if (!fields.TryGetValue(target.IdField, out var idField))
            {
                throw new ArgumentException(
                    $"Published event target '{target.Selector}' references an unknown id field.",
                    nameof(schema));
            }

            if (!idField.Required ||
                idField.Type != EventPayloadDataTypes.String ||
                idField.Format != EventPayloadFormats.Uuid)
            {
                throw new ArgumentException(
                    $"Published event target '{target.Selector}' must reference a required STRING UUID field.",
                    nameof(schema));
            }
        }
    }

    private static void ValidateField(EventPayloadFieldSchema field)
    {
        if (string.IsNullOrWhiteSpace(field.Code))
        {
            throw new ArgumentException("Published event payload schema contains a blank field code.");
        }

        if (field.Type is not (
            EventPayloadDataTypes.String or
            EventPayloadDataTypes.Number or
            EventPayloadDataTypes.Boolean))
        {
            throw new ArgumentException(
                $"Published event field '{field.Code}' has an unsupported type.");
        }

        if (field.Type == EventPayloadDataTypes.String)
        {
            if (field.Format is not null && field.Format != EventPayloadFormats.Uuid)
            {
                throw new ArgumentException(
                    $"Published event field '{field.Code}' has an unsupported format.");
            }

            return;
        }

        if (field.Format is not null)
        {
            throw new ArgumentException(
                $"Published event field '{field.Code}' has an unsupported format.");
        }
    }

    private static void ValidateTarget(EventTargetSchema target)
    {
        if (string.IsNullOrWhiteSpace(target.Selector) ||
            string.IsNullOrWhiteSpace(target.IdField) ||
            target.Kind != EventTargetKinds.Customer)
        {
            throw new ArgumentException("Published event payload schema contains an invalid target.");
        }
    }
}
