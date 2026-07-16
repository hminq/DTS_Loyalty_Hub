using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos.Responses;
using Core.Entities.Constants;
using Core.UseCases.Notifications.Commands;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Notifications;

[ApiController]
[Route("api/admin/notification-templates")]
[Authorize]
public sealed class NotificationTemplatesController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationTemplatesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Notifications.ViewTemplates)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationTemplateResult>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] string? eventTypeCode = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? language = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetNotificationTemplatesQuery(
            page, pageSize, keyword, eventTypeCode, channel, language, isActive), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<NotificationTemplateResult>>
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

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionCodes.Notifications.ViewTemplates)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetNotificationTemplateByIdQuery(id), ct);

        if (result == null)
            return NotFound();

        return Ok(new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.Notifications.CreateTemplate)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> Create(
        [FromBody] CreateNotificationTemplateCommand request,
        CancellationToken ct)
    {
        var actorId = GetActorUserId() ?? Guid.Empty;
        var command = request with { ActorUserId = actorId };

        var result = await _sender.Send(command, ct);

        return Created($"/api/admin/notification-templates/{result.TemplateId}", new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionCodes.Notifications.UpdateTemplate)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateNotificationTemplateCommand request,
        CancellationToken ct)
    {
        var actorId = GetActorUserId() ?? Guid.Empty;
        var command = request with { TemplateId = id, ActorUserId = actorId };

        var result = await _sender.Send(command, ct);

        return Ok(new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Policy = PermissionCodes.Notifications.UpdateTemplate)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> ToggleStatus(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var actorId = GetActorUserId() ?? Guid.Empty;
        var command = new ToggleTemplateStatusCommand(id, actorId);

        var result = await _sender.Send(command, ct);

        return Ok(new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    private Guid? GetActorUserId()
    {
        var rawUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
