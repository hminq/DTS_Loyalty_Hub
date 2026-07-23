using Api.Authentication;
using Api.Dtos.Requests.Tiers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Tiers;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Tiers.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Tiers;

[ApiController]
[Route("api/admin/tiers")]
[Authorize]
public sealed class TiersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<CreateTierRequestDto> _createTierValidator;
    private readonly IValidator<UpdateTierRequestDto> _updateTierValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public TiersController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<CreateTierRequestDto> createTierValidator,
        IValidator<UpdateTierRequestDto> updateTierValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _createTierValidator = createTierValidator;
        _updateTierValidator = updateTierValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Tiers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<TierListItemResponseDto>>>> GetList(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetTierConfigsQuery(), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<TierListItemResponseDto>>
        {
            Data = result.ToListItemResponseDto()
        });
    }

    [HttpGet("{tierConfigId:guid}")]
    [Authorize(Policy = PermissionCodes.Tiers.View)]
    public async Task<ActionResult<ApiResponseDto<TierDetailResponseDto>>> GetById(
        Guid tierConfigId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetTierByIdQuery(tierConfigId), ct);

        return Ok(new ApiResponseDto<TierDetailResponseDto>
        {
            Data = result.ToDetailResponseDto()
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.Tiers.Create)]
    public async Task<ActionResult<ApiResponseDto<TierResponseDto>>> Create(
        [FromBody] CreateTierRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _createTierValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToCommand(_currentAdminContext.UserId), ct);
        var response = new ApiResponseDto<TierResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return CreatedAtAction(
            nameof(GetById),
            new { tierConfigId = response.Data.TierConfigId },
            response);
    }

    [HttpPut("{tierConfigId:guid}")]
    [Authorize(Policy = PermissionCodes.Tiers.Update)]
    public async Task<ActionResult<ApiResponseDto<TierResponseDto>>> Update(
        Guid tierConfigId,
        [FromBody] UpdateTierRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _updateTierValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToCommand(tierConfigId, _currentAdminContext.UserId), ct);

        return Ok(new ApiResponseDto<TierResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

}
