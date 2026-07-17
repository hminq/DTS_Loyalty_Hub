using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos.Requests.Notifications;
using Api.Dtos.Responses;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Notifications;

[ApiController]
[Route("api/admin/notification-event-types")]
[Authorize]
public sealed class NotificationEventTypesController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationEventTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Notifications.ViewEventTypes)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationEventTypeResult>>>> GetList(
        [FromQuery] GetEventTypesRequestDto request,
        [FromServices] IValidator<GetEventTypesRequestDto> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationErrorMapper.FromValidationFailures(validationResult.Errors);
            return BadRequest(ApiErrorResponseDto.Validation(errors));
        }

        var result = await _sender.Send(new GetEventTypesQuery(request.SearchKeyword), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<NotificationEventTypeResult>>
        {
            Data = result
        });
    }
}
