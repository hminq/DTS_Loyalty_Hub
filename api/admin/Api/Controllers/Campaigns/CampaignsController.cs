using Api.Authentication;
using Api.Dtos.Requests.Campaigns;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Campaigns;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Campaigns;

[ApiController]
[Route("api/admin/campaigns")]
[Authorize]
public sealed class CampaignsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<GetCampaignsRequestDto> _getCampaignsValidator;
    private readonly IValidator<CampaignWriteRequestDto> _campaignWriteValidator;
    private readonly IValidator<CampaignActionWriteRequestDto> _campaignActionWriteValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public CampaignsController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetCampaignsRequestDto> getCampaignsValidator,
        IValidator<CampaignWriteRequestDto> campaignWriteValidator,
        IValidator<CampaignActionWriteRequestDto> campaignActionWriteValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getCampaignsValidator = getCampaignsValidator;
        _campaignWriteValidator = campaignWriteValidator;
        _campaignActionWriteValidator = campaignActionWriteValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Campaigns.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CampaignListItemResponseDto>>>> GetList(
        [FromQuery] GetCampaignsRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getCampaignsValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("options")]
    [Authorize(Policy = PermissionCodes.Campaigns.View)]
    public async Task<ActionResult<ApiResponseDto<CampaignOptionsResponseDto>>> GetOptions(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCampaignOptionsQuery(), ct);

        return Ok(new ApiResponseDto<CampaignOptionsResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("{campaignId:guid}")]
    [Authorize(Policy = PermissionCodes.Campaigns.View)]
    public async Task<ActionResult<ApiResponseDto<CampaignDetailResponseDto>>> GetById(
        Guid campaignId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCampaignByIdQuery(campaignId), ct);

        return Ok(new ApiResponseDto<CampaignDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.Campaigns.Create)]
    public async Task<ActionResult<ApiResponseDto<CampaignDetailResponseDto>>> Create(
        [FromBody] CampaignWriteRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _campaignWriteValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToCreateCommand(_currentAdminContext.UserId),
            ct);
        var response = new ApiResponseDto<CampaignDetailResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return CreatedAtAction(
            nameof(GetById),
            new { campaignId = result.CampaignId },
            response);
    }

    [HttpPut("{campaignId:guid}")]
    [Authorize(Policy = PermissionCodes.Campaigns.Update)]
    public async Task<ActionResult<ApiResponseDto<CampaignDetailResponseDto>>> Update(
        Guid campaignId,
        [FromBody] CampaignWriteRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _campaignWriteValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToUpdateCommand(campaignId, _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<CampaignDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpDelete("{campaignId:guid}")]
    [Authorize(Policy = PermissionCodes.Campaigns.Delete)]
    public async Task<IActionResult> Delete(Guid campaignId, CancellationToken ct)
    {
        await _sender.Send(
            new DeleteCampaignCommand(campaignId, _currentAdminContext.UserId),
            ct);

        return NoContent();
    }

    [HttpGet("{campaignId:guid}/actions/{actionId:guid}")]
    [Authorize(Policy = PermissionCodes.Campaigns.View)]
    public async Task<ActionResult<ApiResponseDto<CampaignActionResponseDto>>> GetActionById(
        Guid campaignId,
        Guid actionId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetCampaignActionByIdQuery(campaignId, actionId),
            ct);

        return Ok(new ApiResponseDto<CampaignActionResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost("{campaignId:guid}/actions")]
    [Authorize(Policy = PermissionCodes.Campaigns.Create)]
    public async Task<ActionResult<ApiResponseDto<CampaignActionResponseDto>>> CreateAction(
        Guid campaignId,
        [FromBody] CampaignActionWriteRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _campaignActionWriteValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToCreateCommand(campaignId, _currentAdminContext.UserId),
            ct);
        var response = new ApiResponseDto<CampaignActionResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return CreatedAtAction(
            nameof(GetActionById),
            new { campaignId, actionId = result.ActionId },
            response);
    }

    [HttpPut("{campaignId:guid}/actions/{actionId:guid}")]
    [Authorize(Policy = PermissionCodes.Campaigns.Update)]
    public async Task<ActionResult<ApiResponseDto<CampaignActionResponseDto>>> UpdateAction(
        Guid campaignId,
        Guid actionId,
        [FromBody] CampaignActionWriteRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _campaignActionWriteValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToUpdateCommand(
                campaignId,
                actionId,
                _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<CampaignActionResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpDelete("{campaignId:guid}/actions/{actionId:guid}")]
    [Authorize(Policy = PermissionCodes.Campaigns.Delete)]
    public async Task<IActionResult> DeleteAction(
        Guid campaignId,
        Guid actionId,
        CancellationToken ct)
    {
        await _sender.Send(
            new DeleteCampaignActionCommand(
                campaignId,
                actionId,
                _currentAdminContext.UserId),
            ct);

        return NoContent();
    }
}
