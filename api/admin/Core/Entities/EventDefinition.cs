using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Entities;

public sealed class EventDefinition
{
    private EventDefinition(
        Guid eventTypeId,
        string code,
        string routingKey,
        string name,
        string? description,
        string status,
        DateTime createdAt,
        DateTime updatedAt,
        IReadOnlyCollection<EventDefinitionVersion> versions)
    {
        EventTypeId = eventTypeId;
        Code = code;
        RoutingKey = routingKey;
        Name = name;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Versions = versions;
    }

    public Guid EventTypeId { get; }
    public string Code { get; private set; }
    public string RoutingKey { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<EventDefinitionVersion> Versions { get; private set; }

    public bool HasPublishedVersion => Versions.Any(version =>
        version.Status is EventDefinitionVersionStatuses.Published or EventDefinitionVersionStatuses.Retired);

    public bool HasDraftVersion => Versions.Any(version =>
        version.Status == EventDefinitionVersionStatuses.Draft);

    public bool CanEditIdentity => Status == EventDefinitionStatuses.Active && !HasPublishedVersion;
    public bool CanCreateDraft => Status == EventDefinitionStatuses.Active && !HasDraftVersion;
    public bool CanRetire => Status == EventDefinitionStatuses.Active;

    public static EventDefinition Create(
        string code,
        string routingKey,
        string name,
        string? description,
        DateTime now)
    {
        ValidateIdentity(code, routingKey, name, description);

        return new EventDefinition(
            Guid.NewGuid(),
            code.Trim(),
            routingKey.Trim(),
            name.Trim(),
            NormalizeOptional(description),
            EventDefinitionStatuses.Active,
            now,
            now,
            []);
    }

    public static EventDefinition Restore(
        Guid eventTypeId,
        string code,
        string routingKey,
        string name,
        string? description,
        string status,
        DateTime createdAt,
        DateTime updatedAt,
        IReadOnlyCollection<EventDefinitionVersion> versions)
    {
        return new EventDefinition(
            eventTypeId,
            code,
            routingKey,
            name,
            NormalizeOptional(description),
            status,
            createdAt,
            updatedAt,
            versions);
    }

    public EventDefinitionVersion CreateInitialDraft(string payloadSchema, DateTime now)
    {
        return EventDefinitionVersion.Create(EventTypeId, 1, payloadSchema, now);
    }

    public EventDefinitionVersion CloneVersion(EventDefinitionVersion sourceVersion, DateTime now)
    {
        EnsureActive();

        if (sourceVersion.EventTypeId != EventTypeId)
        {
            throw new DomainException("EVENT_DEFINITION_VERSION_NOT_FOUND", DomainErrorType.NotFound);
        }

        if (sourceVersion.Status != EventDefinitionVersionStatuses.Published)
        {
            throw new DomainException("EVENT_DEFINITION_VERSION_NOT_PUBLISHED", DomainErrorType.Conflict);
        }

        if (HasDraftVersion)
        {
            throw new DomainException("EVENT_DEFINITION_DRAFT_ALREADY_EXISTS", DomainErrorType.Conflict);
        }

        var nextVersion = Versions.Count == 0
            ? 1
            : Versions.Max(version => version.Version) + 1;

        return EventDefinitionVersion.Create(
            EventTypeId,
            nextVersion,
            sourceVersion.PayloadSchema,
            now);
    }

    public void UpdateMetadata(
        string code,
        string routingKey,
        string name,
        string? description,
        DateTime updatedAt)
    {
        EnsureActive();
        ValidateIdentity(code, routingKey, name, description);

        if (HasPublishedVersion &&
            (!Code.Equals(code.Trim(), StringComparison.Ordinal) ||
             !RoutingKey.Equals(routingKey.Trim(), StringComparison.Ordinal)))
        {
            throw new DomainException("EVENT_DEFINITION_IDENTITY_IMMUTABLE", DomainErrorType.Conflict);
        }

        Code = code.Trim();
        RoutingKey = routingKey.Trim();
        Name = name.Trim();
        Description = NormalizeOptional(description);
        UpdatedAt = updatedAt;
    }

    public void Retire(DateTime updatedAt)
    {
        EnsureActive();
        Status = EventDefinitionStatuses.Retired;
        UpdatedAt = updatedAt;
    }

    private void EnsureActive()
    {
        if (Status == EventDefinitionStatuses.Retired)
        {
            throw new DomainException("EVENT_DEFINITION_RETIRED", DomainErrorType.Conflict);
        }
    }

    private static void ValidateIdentity(
        string code,
        string routingKey,
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            code.Trim().Length > EventDefinitionSchemaLimits.MaximumCodeLength)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(routingKey) ||
            routingKey.Trim().Length > EventDefinitionSchemaLimits.MaximumRoutingKeyLength)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(name) ||
            name.Trim().Length > EventDefinitionSchemaLimits.MaximumNameLength)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }

        if (description is not null &&
            description.Trim().Length > EventDefinitionSchemaLimits.MaximumDescriptionLength)
        {
            throw new DomainException("EVENT_DEFINITION_SCHEMA_INVALID", DomainErrorType.Validation);
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
