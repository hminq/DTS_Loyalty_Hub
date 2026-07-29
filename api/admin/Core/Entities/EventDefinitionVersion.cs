using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Entities;

public sealed class EventDefinitionVersion
{
    private EventDefinitionVersion(
        Guid eventTypeVersionId,
        Guid eventTypeId,
        int version,
        string payloadSchema,
        string status,
        DateTime createdAt,
        DateTime updatedAt,
        DateTime? publishedAt)
    {
        EventTypeVersionId = eventTypeVersionId;
        EventTypeId = eventTypeId;
        Version = version;
        PayloadSchema = payloadSchema;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        PublishedAt = publishedAt;
    }

    public Guid EventTypeVersionId { get; }
    public Guid EventTypeId { get; }
    public int Version { get; }
    public string PayloadSchema { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    public bool CanEdit => Status == EventDefinitionVersionStatuses.Draft;
    public bool CanPublish => Status == EventDefinitionVersionStatuses.Draft;
    public bool CanClone => Status == EventDefinitionVersionStatuses.Published;
    public bool CanRetire => Status == EventDefinitionVersionStatuses.Published;

    public static EventDefinitionVersion Create(
        Guid eventTypeId,
        int version,
        string payloadSchema,
        DateTime now)
    {
        return new EventDefinitionVersion(
            Guid.NewGuid(),
            eventTypeId,
            version,
            payloadSchema,
            EventDefinitionVersionStatuses.Draft,
            now,
            now,
            null);
    }

    public static EventDefinitionVersion Restore(
        Guid eventTypeVersionId,
        Guid eventTypeId,
        int version,
        string payloadSchema,
        string status,
        DateTime createdAt,
        DateTime updatedAt,
        DateTime? publishedAt)
    {
        return new EventDefinitionVersion(
            eventTypeVersionId,
            eventTypeId,
            version,
            payloadSchema,
            status,
            createdAt,
            updatedAt,
            publishedAt);
    }

    public void UpdateDraftSchema(string payloadSchema, DateTime updatedAt)
    {
        EnsureDraft();
        PayloadSchema = payloadSchema;
        UpdatedAt = updatedAt;
    }

    public void Publish(DateTime publishedAt)
    {
        EnsureDraft();
        Status = EventDefinitionVersionStatuses.Published;
        PublishedAt = publishedAt;
        UpdatedAt = publishedAt;
    }

    public void Retire(DateTime updatedAt)
    {
        if (Status == EventDefinitionVersionStatuses.Retired)
        {
            throw new DomainException("EVENT_DEFINITION_VERSION_ALREADY_RETIRED", DomainErrorType.Conflict);
        }

        if (Status != EventDefinitionVersionStatuses.Published)
        {
            throw new DomainException("EVENT_DEFINITION_VERSION_NOT_PUBLISHED", DomainErrorType.Conflict);
        }

        Status = EventDefinitionVersionStatuses.Retired;
        UpdatedAt = updatedAt;
    }

    private void EnsureDraft()
    {
        if (Status != EventDefinitionVersionStatuses.Draft)
        {
            throw new DomainException("EVENT_DEFINITION_VERSION_NOT_DRAFT", DomainErrorType.Conflict);
        }
    }
}
