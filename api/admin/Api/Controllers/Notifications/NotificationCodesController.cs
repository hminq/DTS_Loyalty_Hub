using Api.Dtos.Responses;
using Core.Entities.Constants;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Notifications;

[ApiController]
[Route("api/admin/notification-codes")]
[Authorize(Policy = PermissionCodes.Notifications.View)]
public sealed class NotificationCodesController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationCodesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationCodeResult>>>> Get(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetNotificationCodesQuery(), ct);
        return Ok(new ApiResponseDto<IReadOnlyCollection<NotificationCodeResult>>
        {
            Data = result
        });
    }
}
