using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.EventDefinitions.Commands;
using Core.UseCases.EventDefinitions.Results;
using MediatR;

namespace Core.UseCases.EventDefinitions.Handlers;

public sealed class CreateEventDefinitionCommandHandler
    : IRequestHandler<CreateEventDefinitionCommand, EventDefinitionVersionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly EventDefinitionSchemaService _schemaService;
    private readonly TimeProvider _timeProvider;

    public CreateEventDefinitionCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        EventDefinitionSchemaService schemaService,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _schemaService = schemaService;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionVersionDetailResult> Handle(
        CreateEventDefinitionCommand request,
        CancellationToken ct)
    {
        var code = request.Code.Trim();
        var routingKey = request.RoutingKey.Trim();
        await EnsureUniqueIdentityAsync(code, routingKey, null, ct);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var schema = _schemaService.SerializeDraft(request.PayloadSchema);
        var definition = EventDefinition.Create(
            code,
            routingKey,
            request.Name,
            request.Description,
            now);
        var version = definition.CreateInitialDraft(schema, now);

        _repository.Add(definition);
        _repository.AddVersion(version);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.EventType,
            definition.EventTypeId,
            null,
            EventDefinitionAuditSerializer.EventType(definition),
            null));
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.EventTypeVersion,
            version.EventTypeVersionId,
            null,
            EventDefinitionAuditSerializer.EventTypeVersion(version),
            null));

        return ToVersionDetail(definition, version);
    }

    private async Task EnsureUniqueIdentityAsync(
        string code,
        string routingKey,
        Guid? excludingEventTypeId,
        CancellationToken ct)
    {
        if (await _repository.CodeExistsAsync(code, excludingEventTypeId, ct))
        {
            throw new DomainException("EVENT_DEFINITION_CODE_DUPLICATE", DomainErrorType.Conflict);
        }

        if (await _repository.RoutingKeyExistsAsync(routingKey, excludingEventTypeId, ct))
        {
            throw new DomainException("EVENT_DEFINITION_ROUTING_KEY_DUPLICATE", DomainErrorType.Conflict);
        }
    }

    private static EventDefinitionVersionDetailResult ToVersionDetail(
        EventDefinition definition,
        EventDefinitionVersion version)
    {
        return new EventDefinitionVersionDetailResult(
            version.EventTypeVersionId,
            definition.EventTypeId,
            definition.Code,
            version.Version,
            version.Status,
            version.PayloadSchema,
            version.CanEdit,
            version.CanPublish,
            version.CanClone,
            version.CanRetire,
            version.CreatedAt,
            version.UpdatedAt,
            version.PublishedAt);
    }
}

public sealed class UpdateEventDefinitionCommandHandler
    : IRequestHandler<UpdateEventDefinitionCommand, EventDefinitionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public UpdateEventDefinitionCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionDetailResult> Handle(
        UpdateEventDefinitionCommand request,
        CancellationToken ct)
    {
        var definition = await GetAggregateAsync(request.EventTypeId, ct);
        var code = request.Code.Trim();
        var routingKey = request.RoutingKey.Trim();

        if (await _repository.CodeExistsAsync(code, definition.EventTypeId, ct))
        {
            throw new DomainException("EVENT_DEFINITION_CODE_DUPLICATE", DomainErrorType.Conflict);
        }

        if (await _repository.RoutingKeyExistsAsync(routingKey, definition.EventTypeId, ct))
        {
            throw new DomainException("EVENT_DEFINITION_ROUTING_KEY_DUPLICATE", DomainErrorType.Conflict);
        }

        var oldValue = EventDefinitionAuditSerializer.EventType(definition);
        definition.UpdateMetadata(
            code,
            routingKey,
            request.Name,
            request.Description,
            _timeProvider.GetUtcNow().UtcDateTime);

        _repository.Update(definition);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.EventType,
            definition.EventTypeId,
            oldValue,
            EventDefinitionAuditSerializer.EventType(definition),
            null));

        return ToDetail(definition);
    }

    private async Task<EventDefinition> GetAggregateAsync(Guid eventTypeId, CancellationToken ct)
    {
        return await _repository.GetAggregateForUpdateAsync(eventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static EventDefinitionDetailResult ToDetail(EventDefinition definition)
    {
        return new EventDefinitionDetailResult(
            definition.EventTypeId,
            definition.Code,
            definition.RoutingKey,
            definition.Name,
            definition.Description,
            definition.Status,
            definition.CanEditIdentity,
            definition.CanCreateDraft,
            definition.CanRetire,
            definition.CreatedAt,
            definition.UpdatedAt,
            definition.Versions
                .OrderByDescending(version => version.Version)
                .Select(version => new EventDefinitionVersionSummaryResult(
                    version.EventTypeVersionId,
                    version.Version,
                    version.Status,
                    version.CreatedAt,
                    version.UpdatedAt,
                    version.PublishedAt))
                .ToArray());
    }
}

public sealed class UpdateEventDefinitionDraftCommandHandler
    : IRequestHandler<UpdateEventDefinitionDraftCommand, EventDefinitionVersionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly EventDefinitionSchemaService _schemaService;
    private readonly TimeProvider _timeProvider;

    public UpdateEventDefinitionDraftCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        EventDefinitionSchemaService schemaService,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _schemaService = schemaService;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionVersionDetailResult> Handle(
        UpdateEventDefinitionDraftCommand request,
        CancellationToken ct)
    {
        var definition = await GetAggregateAsync(request.EventTypeId, ct);
        EnsureActive(definition);
        var version = GetVersion(definition, request.EventTypeVersionId);
        var oldValue = EventDefinitionAuditSerializer.EventTypeVersion(version);

        version.UpdateDraftSchema(
            _schemaService.SerializeDraft(request.PayloadSchema),
            _timeProvider.GetUtcNow().UtcDateTime);

        _repository.UpdateVersion(version);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.EventTypeVersion,
            version.EventTypeVersionId,
            oldValue,
            EventDefinitionAuditSerializer.EventTypeVersion(version),
            null));

        return ToVersionDetail(definition, version);
    }

    private async Task<EventDefinition> GetAggregateAsync(Guid eventTypeId, CancellationToken ct)
    {
        return await _repository.GetAggregateForUpdateAsync(eventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static void EnsureActive(EventDefinition definition)
    {
        if (definition.Status == EventDefinitionStatuses.Retired)
        {
            throw new DomainException("EVENT_DEFINITION_RETIRED", DomainErrorType.Conflict);
        }
    }

    private static EventDefinitionVersion GetVersion(
        EventDefinition definition,
        Guid eventTypeVersionId)
    {
        return definition.Versions.SingleOrDefault(version =>
                version.EventTypeVersionId == eventTypeVersionId)
            ?? throw new DomainException("EVENT_DEFINITION_VERSION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static EventDefinitionVersionDetailResult ToVersionDetail(
        EventDefinition definition,
        EventDefinitionVersion version)
    {
        return new EventDefinitionVersionDetailResult(
            version.EventTypeVersionId,
            definition.EventTypeId,
            definition.Code,
            version.Version,
            version.Status,
            version.PayloadSchema,
            version.CanEdit,
            version.CanPublish,
            version.CanClone,
            version.CanRetire,
            version.CreatedAt,
            version.UpdatedAt,
            version.PublishedAt);
    }
}

public sealed class PublishEventDefinitionVersionCommandHandler
    : IRequestHandler<PublishEventDefinitionVersionCommand, EventDefinitionVersionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly EventDefinitionSchemaService _schemaService;
    private readonly TimeProvider _timeProvider;

    public PublishEventDefinitionVersionCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        EventDefinitionSchemaService schemaService,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _schemaService = schemaService;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionVersionDetailResult> Handle(
        PublishEventDefinitionVersionCommand request,
        CancellationToken ct)
    {
        var definition = await GetAggregateAsync(request.EventTypeId, ct);
        EnsureActive(definition);
        var version = GetVersion(definition, request.EventTypeVersionId);
        var oldValue = EventDefinitionAuditSerializer.EventTypeVersion(version);
        var publishedAt = _timeProvider.GetUtcNow().UtcDateTime;

        version.UpdateDraftSchema(
            _schemaService.SerializeForPublish(_schemaService.Deserialize(version.PayloadSchema)),
            publishedAt);
        version.Publish(publishedAt);

        _repository.UpdateVersion(version);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Publish,
            AuditEntityTypes.EventTypeVersion,
            version.EventTypeVersionId,
            oldValue,
            EventDefinitionAuditSerializer.EventTypeVersion(version),
            null));

        return ToVersionDetail(definition, version);
    }

    private async Task<EventDefinition> GetAggregateAsync(Guid eventTypeId, CancellationToken ct)
    {
        return await _repository.GetAggregateForUpdateAsync(eventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static void EnsureActive(EventDefinition definition)
    {
        if (definition.Status == EventDefinitionStatuses.Retired)
        {
            throw new DomainException("EVENT_DEFINITION_RETIRED", DomainErrorType.Conflict);
        }
    }

    private static EventDefinitionVersion GetVersion(EventDefinition definition, Guid eventTypeVersionId)
    {
        return definition.Versions.SingleOrDefault(version =>
                version.EventTypeVersionId == eventTypeVersionId)
            ?? throw new DomainException("EVENT_DEFINITION_VERSION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static EventDefinitionVersionDetailResult ToVersionDetail(
        EventDefinition definition,
        EventDefinitionVersion version)
    {
        return new EventDefinitionVersionDetailResult(
            version.EventTypeVersionId,
            definition.EventTypeId,
            definition.Code,
            version.Version,
            version.Status,
            version.PayloadSchema,
            version.CanEdit,
            version.CanPublish,
            version.CanClone,
            version.CanRetire,
            version.CreatedAt,
            version.UpdatedAt,
            version.PublishedAt);
    }
}

public sealed class CloneEventDefinitionVersionCommandHandler
    : IRequestHandler<CloneEventDefinitionVersionCommand, EventDefinitionVersionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public CloneEventDefinitionVersionCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionVersionDetailResult> Handle(
        CloneEventDefinitionVersionCommand request,
        CancellationToken ct)
    {
        var definition = await GetAggregateAsync(request.EventTypeId, ct);
        var source = GetVersion(definition, request.SourceVersionId);
        var draft = definition.CloneVersion(source, _timeProvider.GetUtcNow().UtcDateTime);

        _repository.AddVersion(draft);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.EventTypeVersion,
            draft.EventTypeVersionId,
            null,
            EventDefinitionAuditSerializer.EventTypeVersion(draft),
            EventDefinitionAuditSerializer.CloneMetadata(source.EventTypeVersionId)));

        return ToVersionDetail(definition, draft);
    }

    private async Task<EventDefinition> GetAggregateAsync(Guid eventTypeId, CancellationToken ct)
    {
        return await _repository.GetAggregateForUpdateAsync(eventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static EventDefinitionVersion GetVersion(EventDefinition definition, Guid eventTypeVersionId)
    {
        return definition.Versions.SingleOrDefault(version =>
                version.EventTypeVersionId == eventTypeVersionId)
            ?? throw new DomainException("EVENT_DEFINITION_VERSION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static EventDefinitionVersionDetailResult ToVersionDetail(
        EventDefinition definition,
        EventDefinitionVersion version)
    {
        return new EventDefinitionVersionDetailResult(
            version.EventTypeVersionId,
            definition.EventTypeId,
            definition.Code,
            version.Version,
            version.Status,
            version.PayloadSchema,
            version.CanEdit,
            version.CanPublish,
            version.CanClone,
            version.CanRetire,
            version.CreatedAt,
            version.UpdatedAt,
            version.PublishedAt);
    }
}

public sealed class RetireEventDefinitionVersionCommandHandler
    : IRequestHandler<RetireEventDefinitionVersionCommand, EventDefinitionVersionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public RetireEventDefinitionVersionCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionVersionDetailResult> Handle(
        RetireEventDefinitionVersionCommand request,
        CancellationToken ct)
    {
        var definition = await GetAggregateAsync(request.EventTypeId, ct);
        EnsureActive(definition);
        var version = GetVersion(definition, request.EventTypeVersionId);
        var oldValue = EventDefinitionAuditSerializer.EventTypeVersion(version);

        version.Retire(_timeProvider.GetUtcNow().UtcDateTime);

        _repository.UpdateVersion(version);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Retire,
            AuditEntityTypes.EventTypeVersion,
            version.EventTypeVersionId,
            oldValue,
            EventDefinitionAuditSerializer.EventTypeVersion(version),
            null));

        return ToVersionDetail(definition, version);
    }

    private async Task<EventDefinition> GetAggregateAsync(Guid eventTypeId, CancellationToken ct)
    {
        return await _repository.GetAggregateForUpdateAsync(eventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static void EnsureActive(EventDefinition definition)
    {
        if (definition.Status == EventDefinitionStatuses.Retired)
        {
            throw new DomainException("EVENT_DEFINITION_RETIRED", DomainErrorType.Conflict);
        }
    }

    private static EventDefinitionVersion GetVersion(EventDefinition definition, Guid eventTypeVersionId)
    {
        return definition.Versions.SingleOrDefault(version =>
                version.EventTypeVersionId == eventTypeVersionId)
            ?? throw new DomainException("EVENT_DEFINITION_VERSION_NOT_FOUND", DomainErrorType.NotFound);
    }

    private static EventDefinitionVersionDetailResult ToVersionDetail(
        EventDefinition definition,
        EventDefinitionVersion version)
    {
        return new EventDefinitionVersionDetailResult(
            version.EventTypeVersionId,
            definition.EventTypeId,
            definition.Code,
            version.Version,
            version.Status,
            version.PayloadSchema,
            version.CanEdit,
            version.CanPublish,
            version.CanClone,
            version.CanRetire,
            version.CreatedAt,
            version.UpdatedAt,
            version.PublishedAt);
    }
}

public sealed class RetireEventDefinitionCommandHandler
    : IRequestHandler<RetireEventDefinitionCommand, EventDefinitionDetailResult>
{
    private readonly IEventDefinitionRepository _repository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public RetireEventDefinitionCommandHandler(
        IEventDefinitionRepository repository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<EventDefinitionDetailResult> Handle(
        RetireEventDefinitionCommand request,
        CancellationToken ct)
    {
        var definition = await _repository.GetAggregateForUpdateAsync(request.EventTypeId, ct)
            ?? throw new DomainException("EVENT_DEFINITION_NOT_FOUND", DomainErrorType.NotFound);
        var oldValue = EventDefinitionAuditSerializer.EventType(definition);

        definition.Retire(_timeProvider.GetUtcNow().UtcDateTime);

        _repository.Update(definition);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Retire,
            AuditEntityTypes.EventType,
            definition.EventTypeId,
            oldValue,
            EventDefinitionAuditSerializer.EventType(definition),
            null));

        return ToDetail(definition);
    }

    private static EventDefinitionDetailResult ToDetail(EventDefinition definition)
    {
        return new EventDefinitionDetailResult(
            definition.EventTypeId,
            definition.Code,
            definition.RoutingKey,
            definition.Name,
            definition.Description,
            definition.Status,
            definition.CanEditIdentity,
            definition.CanCreateDraft,
            definition.CanRetire,
            definition.CreatedAt,
            definition.UpdatedAt,
            definition.Versions
                .OrderByDescending(version => version.Version)
                .Select(version => new EventDefinitionVersionSummaryResult(
                    version.EventTypeVersionId,
                    version.Version,
                    version.Status,
                    version.CreatedAt,
                    version.UpdatedAt,
                    version.PublishedAt))
                .ToArray());
    }
}
