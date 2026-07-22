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
    private readonly ValidationErrorMapper _validationErrorMapper;

    public AuditLogsController(
        ISender sender,
        IValidator<GetAuditLogsRequestDto> getAuditLogsValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _getAuditLogsValidator = getAuditLogsValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet("filter-options")]
    [Authorize(Policy = PermissionCodes.AuditLogs.View)]
    public async Task<ActionResult<ApiResponseDto<AuditLogFilterOptionsResponseDto>>> GetFilterOptions(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetAuditLogFilterOptionsQuery(), ct);

        return Ok(new ApiResponseDto<AuditLogFilterOptionsResponseDto>
        {
            Data = result.ToResponseDto()
        });
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
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
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
