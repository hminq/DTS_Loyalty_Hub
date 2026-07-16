using Api.Dtos.Responses;
using Core.Entities.Constants;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationEventTypeResult>>>> GetList([FromQuery] string? searchKeyword, CancellationToken ct)
    {
        var result = await _sender.Send(new GetEventTypesQuery(searchKeyword), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<NotificationEventTypeResult>>
        {
            Data = result
        });
    }
}
