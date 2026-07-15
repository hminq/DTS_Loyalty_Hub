using System.IdentityModel.Tokens.Jwt;
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
    private readonly IValidator<CreateTierRequestDto> _createTierValidator;

    public TiersController(
        ISender sender,
        IValidator<CreateTierRequestDto> createTierValidator)
    {
        _sender = sender;
        _createTierValidator = createTierValidator;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Tiers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<TierResponseDto>>>> GetList(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetTierConfigsQuery(), ct);

        return Ok(new ApiResponseDto<IReadOnlyCollection<TierResponseDto>>
        {
            Data = result.ToResponseDto()
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
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToCommand(GetActorUserId()), ct);
        var response = new ApiResponseDto<TierResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return Created($"/api/admin/tiers/{response.Data.TierConfigId}", response);
    }

    private Guid? GetActorUserId()
    {
        var rawUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}