using Api.Dtos.Requests.VoucherDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.VoucherDefinitions;
using Api.Mappers;
using Core.Entities.Constants;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.VoucherDefinitions;

[ApiController]
[Route("api/admin/voucher-definitions")]
[Authorize]
public sealed class VoucherDefinitionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<GetVoucherDefinitionsRequestDto> _getVoucherDefinitionsValidator;

    public VoucherDefinitionsController(
        ISender sender,
        IValidator<GetVoucherDefinitionsRequestDto> getVoucherDefinitionsValidator)
    {
        _sender = sender;
        _getVoucherDefinitionsValidator = getVoucherDefinitionsValidator;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<VoucherDefinitionResponseDto>>>> GetList(
        [FromQuery] GetVoucherDefinitionsRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getVoucherDefinitionsValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }
}
