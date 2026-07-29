using System.Text.Json;
using Core.Entities;

namespace Core.UseCases.EventDefinitions;

internal static class EventDefinitionAuditSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string EventType(EventDefinition definition)
    {
        return JsonSerializer.Serialize(new
        {
            definition.EventTypeId,
            definition.Code,
            definition.RoutingKey,
            definition.Name,
            definition.Description,
            definition.Status,
            definition.CreatedAt,
            definition.UpdatedAt
        }, JsonOptions);
    }

    public static string EventTypeVersion(EventDefinitionVersion version)
    {
        return JsonSerializer.Serialize(new
        {
            version.EventTypeVersionId,
            version.EventTypeId,
            version.Version,
            PayloadSchema = JsonSerializer.Deserialize<JsonElement>(version.PayloadSchema),
            version.Status,
            version.CreatedAt,
            version.UpdatedAt,
            version.PublishedAt
        }, JsonOptions);
    }

    public static string CloneMetadata(Guid sourceVersionId)
    {
        return JsonSerializer.Serialize(new { sourceVersionId }, JsonOptions);
    }
}
