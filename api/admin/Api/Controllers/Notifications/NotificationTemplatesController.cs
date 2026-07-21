using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Authentication;
using Api.Dtos.Requests.Notifications;
using Api.Dtos.Responses;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Notifications.Commands;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using FluentValidation;
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
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public NotificationTemplatesController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.NotificationTemplates.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<NotificationTemplateResult>>>> GetPaged(
        [FromQuery] GetNotificationTemplatesRequestDto request,
        [FromServices] IValidator<GetNotificationTemplatesRequestDto> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(new GetNotificationTemplatesQuery(
            request.Page, request.PageSize, request.Keyword, request.EventTypeCode, request.Channel, request.Language, request.IsActive), ct);

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
    [Authorize(Policy = PermissionCodes.NotificationTemplates.View)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetNotificationTemplateByIdQuery(id), ct);

        return Ok(new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.NotificationTemplates.Create)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> Create(
        [FromBody] CreateNotificationTemplateRequestDto request,
        [FromServices] IValidator<CreateNotificationTemplateRequestDto> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var command = request.ToCommand(_currentAdminContext.UserId);

        var result = await _sender.Send(command, ct);

        return Created($"/api/admin/notification-templates/{result.TemplateId}", new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionCodes.NotificationTemplates.Update)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateNotificationTemplateRequestDto request,
        [FromServices] IValidator<UpdateNotificationTemplateRequestDto> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var command = request.ToCommand(id, _currentAdminContext.UserId);

        var result = await _sender.Send(command, ct);

        return Ok(new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Policy = PermissionCodes.NotificationTemplates.Update)]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateResult>>> ToggleStatus(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var command = new ToggleTemplateStatusCommand(id, _currentAdminContext.UserId);

        var result = await _sender.Send(command, ct);

        return Ok(new ApiResponseDto<NotificationTemplateResult>
        {
            Data = result
        });
    }
}
