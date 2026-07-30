using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.EventDefinitions.Queries;
using Core.UseCases.EventDefinitions.Results;
using MediatR;

namespace Core.UseCases.EventDefinitions.Handlers;

public sealed class GetEventDefinitionsQueryHandler
    : IRequestHandler<GetEventDefinitionsQuery, PagedResult<EventDefinitionListItemResult>>
{
    private readonly IEventDefinitionRepository _repository;

    public GetEventDefinitionsQueryHandler(IEventDefinitionRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<EventDefinitionListItemResult>> Handle(
        GetEventDefinitionsQuery request,
        CancellationToken ct)
    {
        return _repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.Status,
            ct);
    }
}

public sealed class GetEventDefinitionByIdQueryHandler
    : IRequestHandler<GetEventDefinitionByIdQuery, EventDefinitionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;

    public GetEventDefinitionByIdQueryHandler(IEventDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDefinitionDetailResult> Handle(
        GetEventDefinitionByIdQuery request,
        CancellationToken ct)
    {
        return await _repository.GetDetailAsync(request.EventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
    }
}

public sealed class GetEventDefinitionVersionByIdQueryHandler
    : IRequestHandler<GetEventDefinitionVersionByIdQuery, EventDefinitionVersionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;

    public GetEventDefinitionVersionByIdQueryHandler(IEventDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDefinitionVersionDetailResult> Handle(
        GetEventDefinitionVersionByIdQuery request,
        CancellationToken ct)
    {
        return await _repository.GetVersionDetailAsync(
                request.EventTypeId,
                request.EventTypeVersionId,
                ct)
            ?? throw new DomainException("EVENT_DEFINITION_VERSION_NOT_FOUND", DomainErrorType.NotFound);
    }
}

public sealed class GetEventDefinitionOptionsQueryHandler
    : IRequestHandler<GetEventDefinitionOptionsQuery, EventDefinitionOptionsResult>
{
    private readonly EventDefinitionSchemaService _schemaService;

    public GetEventDefinitionOptionsQueryHandler(EventDefinitionSchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public Task<EventDefinitionOptionsResult> Handle(
        GetEventDefinitionOptionsQuery request,
        CancellationToken ct)
    {
        return Task.FromResult(_schemaService.GetOptions());
    }
}
