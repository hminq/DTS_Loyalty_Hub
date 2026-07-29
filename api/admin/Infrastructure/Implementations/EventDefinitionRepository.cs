using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.UseCases.Common;
using Core.UseCases.EventDefinitions.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;
using DomainEventDefinition = Core.Entities.EventDefinition;
using DomainEventDefinitionVersion = Core.Entities.EventDefinitionVersion;
using PersistenceEventType = Persistence.Models.EventType;
using PersistenceEventTypeVersion = Persistence.Models.EventTypeVersion;

namespace Infrastructure.Implementations;

public sealed class EventDefinitionRepository : IEventDefinitionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public EventDefinitionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<EventDefinitionListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        CancellationToken ct = default)
    {
        var query = _dbContext.EventTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(eventType =>
                EF.Functions.ILike(eventType.Code, pattern) ||
                EF.Functions.ILike(eventType.RoutingKey, pattern) ||
                EF.Functions.ILike(eventType.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(eventType => eventType.Status == status);
        }

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(eventType => eventType.UpdatedAt)
            .ThenBy(eventType => eventType.EventTypeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(eventType => new EventDefinitionListItemResult(
                eventType.EventTypeId,
                eventType.Code,
                eventType.RoutingKey,
                eventType.Name,
                eventType.Description,
                eventType.Status,
                eventType.EventTypeVersions
                    .Select(version => (int?)version.Version)
                    .Max(),
                eventType.EventTypeVersions
                    .Where(version => version.Status == EventDefinitionVersionStatuses.Published)
                    .Select(version => (int?)version.Version)
                    .Max(),
                eventType.EventTypeVersions
                    .Where(version => version.Status == EventDefinitionVersionStatuses.Draft)
                    .Select(version => (int?)version.Version)
                    .SingleOrDefault(),
                eventType.CreatedAt,
                eventType.UpdatedAt))
            .ToArrayAsync(ct);

        return new PagedResult<EventDefinitionListItemResult>(items, page, pageSize, totalItems);
    }

    public async Task<EventDefinitionDetailResult?> GetDetailAsync(
        Guid eventTypeId,
        CancellationToken ct = default)
    {
        var eventType = await _dbContext.EventTypes
            .AsNoTracking()
            .Where(item => item.EventTypeId == eventTypeId)
            .Select(item => new
            {
                item.EventTypeId,
                item.Code,
                item.RoutingKey,
                item.Name,
                item.Description,
                item.Status,
                item.CreatedAt,
                item.UpdatedAt
            })
            .SingleOrDefaultAsync(ct);

        if (eventType is null)
        {
            return null;
        }

        var versions = await _dbContext.EventTypeVersions
            .AsNoTracking()
            .Where(version => version.EventTypeId == eventTypeId)
            .OrderByDescending(version => version.Version)
            .Select(version => new EventDefinitionVersionSummaryResult(
                version.EventTypeVersionId,
                version.Version,
                version.Status,
                version.CreatedAt,
                version.UpdatedAt,
                version.PublishedAt))
            .ToArrayAsync(ct);

        var hasPublished = versions.Any(version =>
            version.Status is EventDefinitionVersionStatuses.Published or EventDefinitionVersionStatuses.Retired);
        var hasDraft = versions.Any(version =>
            version.Status == EventDefinitionVersionStatuses.Draft);
        var isActive = eventType.Status == EventDefinitionStatuses.Active;

        return new EventDefinitionDetailResult(
            eventType.EventTypeId,
            eventType.Code,
            eventType.RoutingKey,
            eventType.Name,
            eventType.Description,
            eventType.Status,
            isActive && !hasPublished,
            isActive && !hasDraft,
            isActive,
            eventType.CreatedAt,
            eventType.UpdatedAt,
            versions);
    }

    public Task<EventDefinitionVersionDetailResult?> GetVersionDetailAsync(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        CancellationToken ct = default)
    {
        return _dbContext.EventTypeVersions
            .AsNoTracking()
            .Where(version =>
                version.EventTypeId == eventTypeId &&
                version.EventTypeVersionId == eventTypeVersionId)
            .Select(version => new EventDefinitionVersionDetailResult(
                version.EventTypeVersionId,
                version.EventTypeId,
                version.EventType.Code,
                version.Version,
                version.Status,
                version.PayloadSchema,
                version.Status == EventDefinitionVersionStatuses.Draft,
                version.Status == EventDefinitionVersionStatuses.Draft,
                version.Status == EventDefinitionVersionStatuses.Published,
                version.Status == EventDefinitionVersionStatuses.Published,
                version.CreatedAt,
                version.UpdatedAt,
                version.PublishedAt))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<DomainEventDefinition?> GetAggregateForUpdateAsync(
        Guid eventTypeId,
        CancellationToken ct = default)
    {
        var lockedType = await _dbContext.EventTypes
            .FromSqlInterpolated($"SELECT * FROM event_types WHERE event_type_id = {eventTypeId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(ct);

        if (lockedType is null)
        {
            return null;
        }

        var versions = await _dbContext.EventTypeVersions
            .AsNoTracking()
            .Where(version => version.EventTypeId == eventTypeId)
            .OrderBy(version => version.Version)
            .Select(version => DomainEventDefinitionVersion.Restore(
                version.EventTypeVersionId,
                version.EventTypeId,
                version.Version,
                version.PayloadSchema,
                version.Status,
                version.CreatedAt,
                version.UpdatedAt,
                version.PublishedAt))
            .ToArrayAsync(ct);

        return DomainEventDefinition.Restore(
            lockedType.EventTypeId,
            lockedType.Code,
            lockedType.RoutingKey,
            lockedType.Name,
            lockedType.Description,
            lockedType.Status,
            lockedType.CreatedAt,
            lockedType.UpdatedAt,
            versions);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludingEventTypeId,
        CancellationToken ct = default)
    {
        return _dbContext.EventTypes
            .AsNoTracking()
            .AnyAsync(eventType =>
                eventType.Code == code &&
                (!excludingEventTypeId.HasValue || eventType.EventTypeId != excludingEventTypeId.Value),
                ct);
    }

    public Task<bool> RoutingKeyExistsAsync(
        string routingKey,
        Guid? excludingEventTypeId,
        CancellationToken ct = default)
    {
        return _dbContext.EventTypes
            .AsNoTracking()
            .AnyAsync(eventType =>
                eventType.RoutingKey == routingKey &&
                (!excludingEventTypeId.HasValue || eventType.EventTypeId != excludingEventTypeId.Value),
                ct);
    }

    public void Add(DomainEventDefinition definition)
    {
        _dbContext.EventTypes.Add(ToPersistence(definition));
    }

    public void AddVersion(DomainEventDefinitionVersion version)
    {
        _dbContext.EventTypeVersions.Add(ToPersistence(version));
    }

    public void Update(DomainEventDefinition definition)
    {
        _dbContext.EventTypes.Update(ToPersistence(definition));
    }

    public void UpdateVersion(DomainEventDefinitionVersion version)
    {
        _dbContext.EventTypeVersions.Update(ToPersistence(version));
    }

    private static PersistenceEventType ToPersistence(DomainEventDefinition definition)
    {
        return new PersistenceEventType
        {
            EventTypeId = definition.EventTypeId,
            Code = definition.Code,
            RoutingKey = definition.RoutingKey,
            Name = definition.Name,
            Description = definition.Description,
            Status = definition.Status,
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.UpdatedAt
        };
    }

    private static PersistenceEventTypeVersion ToPersistence(DomainEventDefinitionVersion version)
    {
        return new PersistenceEventTypeVersion
        {
            EventTypeVersionId = version.EventTypeVersionId,
            EventTypeId = version.EventTypeId,
            Version = version.Version,
            PayloadSchema = version.PayloadSchema,
            Status = version.Status,
            CreatedAt = version.CreatedAt,
            UpdatedAt = version.UpdatedAt,
            PublishedAt = version.PublishedAt
        };
    }
}
