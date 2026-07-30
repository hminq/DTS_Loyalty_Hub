using Core.Abstractions;
using Core.UseCases.EventDefinitions.Results;
using MediatR;
using Messaging.Contracts.Events;

namespace Core.UseCases.EventDefinitions.Commands;

public sealed record CreateEventDefinitionCommand(
    string Code,
    string RoutingKey,
    string Name,
    string? Description,
    EventPayloadSchema PayloadSchema,
    Guid? ActorUserId) : IRequest<EventDefinitionVersionDetailResult>, ITransactionalRequest;

public sealed record UpdateEventDefinitionCommand(
    Guid EventTypeId,
    string Code,
    string RoutingKey,
    string Name,
    string? Description,
    Guid? ActorUserId) : IRequest<EventDefinitionDetailResult>, ITransactionalRequest;

public sealed record UpdateEventDefinitionDraftCommand(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    EventPayloadSchema PayloadSchema,
    Guid? ActorUserId) : IRequest<EventDefinitionVersionDetailResult>, ITransactionalRequest;

public sealed record PublishEventDefinitionVersionCommand(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    Guid? ActorUserId) : IRequest<EventDefinitionVersionDetailResult>, ITransactionalRequest;

public sealed record CloneEventDefinitionVersionCommand(
    Guid EventTypeId,
    Guid SourceVersionId,
    Guid? ActorUserId) : IRequest<EventDefinitionVersionDetailResult>, ITransactionalRequest;

public sealed record RetireEventDefinitionVersionCommand(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    Guid? ActorUserId) : IRequest<EventDefinitionVersionDetailResult>, ITransactionalRequest;

public sealed record RetireEventDefinitionCommand(
    Guid EventTypeId,
    Guid? ActorUserId) : IRequest<EventDefinitionDetailResult>, ITransactionalRequest;
