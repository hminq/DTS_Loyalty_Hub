using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos.Requests.Notifications;
using Api.Dtos.Responses;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Common;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Notifications;

[ApiController]
[Route("api/admin/notification-logs")]
[Authorize]
public sealed class NotificationLogsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public NotificationLogsController(
        ISender sender,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Notifications.ViewLogs)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationLogResult>>>> GetPaged(
        [FromQuery] GetNotificationLogsRequestDto request,
        [FromServices] IValidator<GetNotificationLogsRequestDto> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(new GetNotificationLogsQuery(request.Page, request.PageSize, request.CustomerId, request.EventTypeCode), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<NotificationLogResult>>
        {
            Data = result.Items,
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        });
    }
}
