using Api.Authentication;
using Api.Dtos.Requests.VoucherDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.VoucherDefinitions;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.VoucherDefinitions.Queries;
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
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<GetVoucherDefinitionsRequestDto> _getVoucherDefinitionsValidator;
    private readonly IValidator<CreateVoucherDefinitionRequestDto> _createVoucherDefinitionValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public VoucherDefinitionsController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetVoucherDefinitionsRequestDto> getVoucherDefinitionsValidator,
        IValidator<CreateVoucherDefinitionRequestDto> createVoucherDefinitionValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getVoucherDefinitionsValidator = getVoucherDefinitionsValidator;
        _createVoucherDefinitionValidator = createVoucherDefinitionValidator;
        _validationErrorMapper = validationErrorMapper;
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
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("{voucherDefinitionId:guid}")]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<VoucherDefinitionResponseDto>>> GetById(
        Guid voucherDefinitionId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetVoucherDefinitionByIdQuery(voucherDefinitionId),
            ct);

        return Ok(new ApiResponseDto<VoucherDefinitionResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.Create)]
    public async Task<ActionResult<ApiResponseDto<VoucherDefinitionResponseDto>>> Create(
        [FromBody] CreateVoucherDefinitionRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _createVoucherDefinitionValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToCommand(_currentAdminContext.UserId), ct);
        var response = new ApiResponseDto<VoucherDefinitionResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return CreatedAtAction(
            nameof(GetById),
            new { voucherDefinitionId = result.VoucherDefinitionId },
            response);
    }

}
