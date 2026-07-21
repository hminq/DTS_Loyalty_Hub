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
    private readonly ValidationErrorMapper _validationErrorMapper;

    public NotificationEventTypesController(
        ISender sender,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.NotificationEventTypes.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationEventTypeResult>>>> GetList(
        [FromQuery] GetEventTypesRequestDto request,
        [FromServices] IValidator<GetEventTypesRequestDto> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(new GetEventTypesQuery(request.SearchKeyword), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<NotificationEventTypeResult>>
        {
            Data = result
        });
    }
}
