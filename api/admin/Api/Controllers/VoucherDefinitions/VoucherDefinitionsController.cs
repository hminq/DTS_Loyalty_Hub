using Api.Authentication;
using Api.Dtos.Requests.VoucherDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.VoucherDefinitions;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Commands;
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
    private readonly IValidator<CreateVoucherPoolImportUploadUrlRequestDto> _importUploadValidator;
    private readonly IValidator<CreateVoucherPoolImportJobRequestDto> _importJobValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public VoucherDefinitionsController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetVoucherDefinitionsRequestDto> getVoucherDefinitionsValidator,
        IValidator<CreateVoucherDefinitionRequestDto> createVoucherDefinitionValidator,
        IValidator<CreateVoucherPoolImportUploadUrlRequestDto> importUploadValidator,
        IValidator<CreateVoucherPoolImportJobRequestDto> importJobValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getVoucherDefinitionsValidator = getVoucherDefinitionsValidator;
        _createVoucherDefinitionValidator = createVoucherDefinitionValidator;
        _importUploadValidator = importUploadValidator;
        _importJobValidator = importJobValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpPost("{voucherDefinitionId:guid}/pool-imports/upload-url")]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<VoucherPoolImportUploadResponseDto>>> CreateImportUploadUrl(
        Guid voucherDefinitionId,
        [FromBody] CreateVoucherPoolImportUploadUrlRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _importUploadValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new CreateVoucherPoolImportUploadUrlQuery(
                voucherDefinitionId,
                request.FileName,
                request.FileSizeBytes),
            ct);

        return Ok(new ApiResponseDto<VoucherPoolImportUploadResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost("{voucherDefinitionId:guid}/pool-imports")]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<VoucherPoolProvisioningResponseDto>>> CreateImportJob(
        Guid voucherDefinitionId,
        [FromBody] CreateVoucherPoolImportJobRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _importJobValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            new CreateVoucherPoolImportJobCommand(
                voucherDefinitionId,
                request.ImportFileKey,
                _currentAdminContext.UserId),
            ct);
        var response = new ApiResponseDto<VoucherPoolProvisioningResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return AcceptedAtAction(
            nameof(GetById),
            new { voucherDefinitionId },
            response);
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<VoucherDefinitionListItemResponseDto>>>> GetList(
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

    [HttpGet("options")]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<VoucherDefinitionOptionsResponseDto>>> GetOptions(CancellationToken ct)
    {
        var result = await _sender.Send(new GetVoucherDefinitionOptionsQuery(), ct);

        return Ok(new ApiResponseDto<VoucherDefinitionOptionsResponseDto>
        {
            Data = result.ToOptionsResponseDto()
        });
    }

    [HttpGet("import-template")]
    [Authorize(Policy = PermissionCodes.VoucherDefinitions.Create)]
    public async Task<ActionResult<ApiResponseDto<VoucherImportTemplateResponseDto>>> GetImportTemplate(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetVoucherImportTemplateQuery(), ct);

        return Ok(new ApiResponseDto<VoucherImportTemplateResponseDto>
        {
            Data = result.ToResponseDto()
        });
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
