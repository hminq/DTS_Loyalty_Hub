using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Dtos.Requests.EventDefinitions;

public sealed class GetEventDefinitionsRequestDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }

    public string? Status { get; set; }
}

public class EventDefinitionWriteRequestDto
{
    public string Code { get; set; } = null!;

    public string RoutingKey { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}

public sealed class CreateEventDefinitionRequestDto : EventDefinitionWriteRequestDto
{
    public EventPayloadSchemaRequestDto? PayloadSchema { get; set; }
}

public sealed class UpdateEventDefinitionDraftRequestDto
{
    public EventPayloadSchemaRequestDto? PayloadSchema { get; set; }
}

public sealed class EventPayloadSchemaRequestDto
{
    public IReadOnlyCollection<EventPayloadFieldSchemaRequestDto>? Fields { get; set; }

    public IReadOnlyCollection<EventTargetSchemaRequestDto>? Targets { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class EventPayloadFieldSchemaRequestDto
{
    public string Code { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? Format { get; set; }

    public bool Required { get; set; }

    public bool Conditionable { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class EventTargetSchemaRequestDto
{
    public string Selector { get; set; } = null!;

    public string Kind { get; set; } = null!;

    public string IdField { get; set; } = null!;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
