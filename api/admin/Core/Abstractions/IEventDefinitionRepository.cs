using Core.Entities;
using Core.UseCases.Common;
using Core.UseCases.EventDefinitions.Results;

namespace Core.Abstractions;

public interface IEventDefinitionRepository
{
    Task<PagedResult<EventDefinitionListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        CancellationToken ct = default);

    Task<EventDefinitionDetailResult?> GetDetailAsync(Guid eventTypeId, CancellationToken ct = default);

    Task<EventDefinitionVersionDetailResult?> GetVersionDetailAsync(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        CancellationToken ct = default);

    Task<EventDefinition?> GetAggregateForUpdateAsync(Guid eventTypeId, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, Guid? excludingEventTypeId, CancellationToken ct = default);

    Task<bool> RoutingKeyExistsAsync(string routingKey, Guid? excludingEventTypeId, CancellationToken ct = default);

    void Add(EventDefinition definition);

    void AddVersion(EventDefinitionVersion version);

    void Update(EventDefinition definition);

    void UpdateVersion(EventDefinitionVersion version);
}
