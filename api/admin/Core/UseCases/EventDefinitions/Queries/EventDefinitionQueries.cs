using Core.UseCases.Common;
using Core.UseCases.EventDefinitions.Results;
using MediatR;

namespace Core.UseCases.EventDefinitions.Queries;

public sealed record GetEventDefinitionsQuery(
    int Page,
    int PageSize,
    string? Keyword,
    string? Status) : IRequest<PagedResult<EventDefinitionListItemResult>>;

public sealed record GetEventDefinitionByIdQuery(Guid EventTypeId)
    : IRequest<EventDefinitionDetailResult>;

public sealed record GetEventDefinitionVersionByIdQuery(
    Guid EventTypeId,
    Guid EventTypeVersionId) : IRequest<EventDefinitionVersionDetailResult>;

public sealed record GetEventDefinitionOptionsQuery : IRequest<EventDefinitionOptionsResult>;
