using System.Text.Json;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Definitions;
using Messaging.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Consumer.Infrastructure.Implementations;

public sealed class EfEventDefinitionStore : IEventDefinitionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly LoyaltyHubDbContext _dbContext;

    public EfEventDefinitionStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PublishedEventDefinition?> FetchDefinitionAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventType) || eventVersion <= 0)
        {
            return null;
        }

        var record = await (
            from etv in _dbContext.EventTypeVersions.AsNoTracking()
            join et in _dbContext.EventTypes.AsNoTracking()
                on etv.EventTypeId equals et.EventTypeId
            where et.Code == eventType
                && etv.Version == eventVersion
                && (etv.Status == "PUBLISHED" || etv.Status == "RETIRED")
                && etv.PublishedAt != null
            select new
            {
                et.EventTypeId,
                etv.EventTypeVersionId,
                EventTypeCode = et.Code,
                et.RoutingKey,
                etv.Version,
                etv.Status,
                PublishedAt = etv.PublishedAt!.Value,
                etv.PayloadSchema
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (record is null || string.IsNullOrWhiteSpace(record.PayloadSchema))
        {
            return null;
        }

        EventPayloadSchema? schema;
        try
        {
            schema = JsonSerializer.Deserialize<EventPayloadSchema>(
                record.PayloadSchema,
                JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (schema is null || schema.Fields is null || schema.Targets is null)
        {
            return null;
        }

        try
        {
            return new PublishedEventDefinition(
                record.EventTypeId,
                record.EventTypeVersionId,
                record.EventTypeCode,
                record.RoutingKey,
                record.Version,
                record.Status,
                record.PublishedAt,
                schema);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
