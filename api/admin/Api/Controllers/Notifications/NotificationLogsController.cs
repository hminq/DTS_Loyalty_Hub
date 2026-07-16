using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos.Responses;
using Core.Entities.Constants;
using Core.UseCases.Common;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
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

    public NotificationLogsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Notifications.ViewLogs)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationLogResult>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? eventTypeCode = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetNotificationLogsQuery(page, pageSize, customerId, eventTypeCode), ct);

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
