using Api.Dtos.Requests.AuditLogs;
using Api.Dtos.Responses;
using Api.Dtos.Responses.AuditLogs;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.AuditLogs.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AuditLogs;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize]
public sealed class AuditLogsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<GetAuditLogsRequestDto> _getAuditLogsValidator;

    public AuditLogsController(
        ISender sender,
        IValidator<GetAuditLogsRequestDto> getAuditLogsValidator)
    {
        _sender = sender;
        _getAuditLogsValidator = getAuditLogsValidator;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.AuditLogs.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<AuditLogResponseDto>>>> GetList(
        [FromQuery] GetAuditLogsRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getAuditLogsValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(
            new GetAuditLogsQuery(
                request.Page,
                request.PageSize,
                request.FromDate,
                request.ToDate,
                request.EntityType,
                request.Action),
            ct);

        return Ok(result.ToPagedResponseDto());
    }
}
