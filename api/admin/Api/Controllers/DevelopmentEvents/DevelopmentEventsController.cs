using System.Text.Json;
using Api.Dtos.Requests.DevelopmentEvents;
using Api.Dtos.Responses;
using Api.Dtos.Responses.DevelopmentEvents;
using Api.Mappers;
using Core.Abstractions;
using Core.Entities.Events;
using FluentValidation;
using Messaging.Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.DevelopmentEvents;

[ApiController]
[AllowAnonymous]
[Route("api/admin/dev/mock-events")]
public sealed class DevelopmentEventsController : ControllerBase
{
    private const int EventVersion = 1;

    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<CustomerMissionCompletedRequestDto> _validator;
    private readonly ValidationErrorMapper _validationErrorMapper;
    private readonly TimeProvider _timeProvider;
    private readonly IHostEnvironment _hostEnvironment;

    public DevelopmentEventsController(
        IEventPublisher eventPublisher,
        IValidator<CustomerMissionCompletedRequestDto> validator,
        ValidationErrorMapper validationErrorMapper,
        TimeProvider timeProvider,
        IHostEnvironment hostEnvironment)
    {
        _eventPublisher = eventPublisher;
        _validator = validator;
        _validationErrorMapper = validationErrorMapper;
        _timeProvider = timeProvider;
        _hostEnvironment = hostEnvironment;
    }

    [HttpPost("customer-mission-completed")]
    public async Task<ActionResult<ApiResponseDto<MockEventPublishedResponseDto>>> PublishCustomerMissionCompleted(
        [FromBody] CustomerMissionCompletedRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!_hostEnvironment.IsDevelopment())
        {
            return NotFound();
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var eventId = Guid.NewGuid();
        var occurredAt = _timeProvider.GetUtcNow().UtcDateTime;
        var payload = new CustomerMissionCompletedPayload(
            request.CustomerId,
            request.MissionCode,
            request.MissionCategory,
            request.IsFirstCompletion!.Value,
            request.CompletionCount);
        var envelope = new EventEnvelope<CustomerMissionCompletedPayload>(
            eventId,
            EventTypeCodes.CustomerMissionCompleted,
            EventVersion,
            occurredAt,
            payload);
        var body = JsonSerializer.Serialize(envelope, JsonSerializerOptions.Web);

        await _eventPublisher.PublishAsync(
            new OutgoingEvent(
                eventId,
                EventTypeCodes.CustomerMissionCompleted,
                EventRoutingKeys.CustomerMissionCompleted,
                body),
            cancellationToken);

        return Ok(new ApiResponseDto<MockEventPublishedResponseDto>
        {
            Data = new MockEventPublishedResponseDto
            {
                EventId = eventId,
                EventType = EventTypeCodes.CustomerMissionCompleted,
                EventVersion = EventVersion,
                OccurredAt = occurredAt,
                RoutingKey = EventRoutingKeys.CustomerMissionCompleted,
                Payload = payload
            }
        });
    }
}
