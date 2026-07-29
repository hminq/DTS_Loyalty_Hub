using System.Text.Json;
using Api.Dtos.Requests.EventDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.EventDefinitions;
using Core.UseCases.Common;
using Core.UseCases.EventDefinitions;
using Core.UseCases.EventDefinitions.Commands;
using Core.UseCases.EventDefinitions.Queries;
using Core.UseCases.EventDefinitions.Results;
using Messaging.Contracts.Events;

namespace Api.Mappers;

public static class EventDefinitionMapper
{
    public static GetEventDefinitionsQuery ToQuery(this GetEventDefinitionsRequestDto request)
    {
        return new GetEventDefinitionsQuery(
            request.Page,
            request.PageSize,
            request.Keyword,
            string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim());
    }

    public static CreateEventDefinitionCommand ToCreateCommand(
        this CreateEventDefinitionRequestDto request,
        Guid? actorUserId)
    {
        return new CreateEventDefinitionCommand(
            request.Code,
            request.RoutingKey,
            request.Name,
            request.Description,
            request.PayloadSchema!.ToContract(),
            actorUserId);
    }

    public static UpdateEventDefinitionCommand ToUpdateCommand(
        this EventDefinitionWriteRequestDto request,
        Guid eventTypeId,
        Guid? actorUserId)
    {
        return new UpdateEventDefinitionCommand(
            eventTypeId,
            request.Code,
            request.RoutingKey,
            request.Name,
            request.Description,
            actorUserId);
    }

    public static UpdateEventDefinitionDraftCommand ToUpdateDraftCommand(
        this UpdateEventDefinitionDraftRequestDto request,
        Guid eventTypeId,
        Guid eventTypeVersionId,
        Guid? actorUserId)
    {
        return new UpdateEventDefinitionDraftCommand(
            eventTypeId,
            eventTypeVersionId,
            request.PayloadSchema!.ToContract(),
            actorUserId);
    }

    public static ApiResponseDto<IReadOnlyCollection<EventDefinitionListItemResponseDto>> ToPagedResponseDto(
        this PagedResult<EventDefinitionListItemResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<EventDefinitionListItemResponseDto>>
        {
            Data = result.Items.Select(item => new EventDefinitionListItemResponseDto(
                item.EventTypeId,
                item.Code,
                item.RoutingKey,
                item.Name,
                item.Description,
                item.Status,
                item.LatestVersion,
                item.LatestPublishedVersion,
                item.DraftVersion,
                ToUtcOffset(item.CreatedAt),
                ToUtcOffset(item.UpdatedAt)))
                .ToArray(),
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }

    public static EventDefinitionDetailResponseDto ToResponseDto(
        this EventDefinitionDetailResult result)
    {
        return new EventDefinitionDetailResponseDto(
            result.EventTypeId,
            result.Code,
            result.RoutingKey,
            result.Name,
            result.Description,
            result.Status,
            result.CanEditIdentity,
            result.CanCreateDraft,
            result.CanRetire,
            ToUtcOffset(result.CreatedAt),
            ToUtcOffset(result.UpdatedAt),
            result.Versions.Select(version => new EventDefinitionVersionSummaryResponseDto(
                version.EventTypeVersionId,
                version.Version,
                version.Status,
                ToUtcOffset(version.CreatedAt),
                ToUtcOffset(version.UpdatedAt),
                version.PublishedAt.HasValue ? ToUtcOffset(version.PublishedAt.Value) : null))
                .ToArray());
    }

    public static EventDefinitionVersionDetailResponseDto ToResponseDto(
        this EventDefinitionVersionDetailResult result)
    {
        return new EventDefinitionVersionDetailResponseDto(
            result.EventTypeVersionId,
            result.EventTypeId,
            result.EventTypeCode,
            result.Version,
            result.Status,
            JsonSerializer.Deserialize<JsonElement>(result.PayloadSchema),
            result.CanEdit,
            result.CanPublish,
            result.CanClone,
            result.CanRetire,
            ToUtcOffset(result.CreatedAt),
            ToUtcOffset(result.UpdatedAt),
            result.PublishedAt.HasValue ? ToUtcOffset(result.PublishedAt.Value) : null);
    }

    public static EventDefinitionOptionsResponseDto ToResponseDto(
        this EventDefinitionOptionsResult result)
    {
        return new EventDefinitionOptionsResponseDto(
            result.FieldTypes.Select(fieldType => new EventDefinitionFieldTypeOptionResponseDto(
                fieldType.Code,
                fieldType.Formats,
                fieldType.Operators)).ToArray(),
            result.TargetKinds.Select(targetKind => new EventDefinitionTargetKindOptionResponseDto(
                targetKind.Code,
                targetKind.IdentityFieldType,
                targetKind.IdentityFieldFormat)).ToArray(),
            result.EventTypeStatuses,
            result.VersionStatuses,
            new EventDefinitionLimitsResponseDto(
                result.Limits.MaximumFields,
                result.Limits.MaximumTargets,
                result.Limits.MaximumSchemaBytes));
    }

    private static EventPayloadSchema ToContract(this EventPayloadSchemaRequestDto schema)
    {
        return new EventPayloadSchema(
            schema.Fields?.Select(field => new EventPayloadFieldSchema(
                field.Code,
                field.Type,
                field.Format,
                field.Required,
                field.Conditionable)).ToArray() ?? [],
            schema.Targets?.Select(target => new EventTargetSchema(
                target.Selector,
                target.Kind,
                target.IdField)).ToArray() ?? []);
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
