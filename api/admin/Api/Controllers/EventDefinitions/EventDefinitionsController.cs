using Api.Authentication;
using Api.Dtos.Requests.EventDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.EventDefinitions;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.EventDefinitions.Commands;
using Core.UseCases.EventDefinitions.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.EventDefinitions;

[ApiController]
[Route("api/admin/event-definitions")]
[Authorize]
public sealed class EventDefinitionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<GetEventDefinitionsRequestDto> _getValidator;
    private readonly IValidator<CreateEventDefinitionRequestDto> _createValidator;
    private readonly IValidator<EventDefinitionWriteRequestDto> _writeValidator;
    private readonly IValidator<UpdateEventDefinitionDraftRequestDto> _draftValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public EventDefinitionsController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetEventDefinitionsRequestDto> getValidator,
        IValidator<CreateEventDefinitionRequestDto> createValidator,
        IValidator<EventDefinitionWriteRequestDto> writeValidator,
        IValidator<UpdateEventDefinitionDraftRequestDto> draftValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getValidator = getValidator;
        _createValidator = createValidator;
        _writeValidator = writeValidator;
        _draftValidator = draftValidator;
        _validationErrorMapper = validationErrorMapper;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.EventDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<EventDefinitionListItemResponseDto>>>> GetList(
        [FromQuery] GetEventDefinitionsRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("options")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionOptionsResponseDto>>> GetOptions(
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetEventDefinitionOptionsQuery(), ct);

        return Ok(new ApiResponseDto<EventDefinitionOptionsResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("{eventTypeId:guid}")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionDetailResponseDto>>> GetById(
        Guid eventTypeId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetEventDefinitionByIdQuery(eventTypeId), ct);

        return Ok(new ApiResponseDto<EventDefinitionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpGet("{eventTypeId:guid}/versions/{eventTypeVersionId:guid}")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.View)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionVersionDetailResponseDto>>> GetVersionById(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetEventDefinitionVersionByIdQuery(eventTypeId, eventTypeVersionId),
            ct);

        return Ok(new ApiResponseDto<EventDefinitionVersionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Create)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionVersionDetailResponseDto>>> Create(
        [FromBody] CreateEventDefinitionRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _createValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToCreateCommand(_currentAdminContext.UserId),
            ct);
        var response = new ApiResponseDto<EventDefinitionVersionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return CreatedAtAction(
            nameof(GetVersionById),
            new
            {
                eventTypeId = result.EventTypeId,
                eventTypeVersionId = result.EventTypeVersionId
            },
            response);
    }

    [HttpPut("{eventTypeId:guid}")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionDetailResponseDto>>> Update(
        Guid eventTypeId,
        [FromBody] EventDefinitionWriteRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _writeValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToUpdateCommand(eventTypeId, _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<EventDefinitionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPut("{eventTypeId:guid}/versions/{eventTypeVersionId:guid}")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionVersionDetailResponseDto>>> UpdateDraft(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        [FromBody] UpdateEventDefinitionDraftRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _draftValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToUpdateDraftCommand(
                eventTypeId,
                eventTypeVersionId,
                _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<EventDefinitionVersionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost("{eventTypeId:guid}/versions/{eventTypeVersionId:guid}/publish")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionVersionDetailResponseDto>>> Publish(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new PublishEventDefinitionVersionCommand(
                eventTypeId,
                eventTypeVersionId,
                _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<EventDefinitionVersionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost("{eventTypeId:guid}/versions/{sourceVersionId:guid}/clone")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionVersionDetailResponseDto>>> Clone(
        Guid eventTypeId,
        Guid sourceVersionId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new CloneEventDefinitionVersionCommand(
                eventTypeId,
                sourceVersionId,
                _currentAdminContext.UserId),
            ct);
        var response = new ApiResponseDto<EventDefinitionVersionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return CreatedAtAction(
            nameof(GetVersionById),
            new
            {
                eventTypeId = result.EventTypeId,
                eventTypeVersionId = result.EventTypeVersionId
            },
            response);
    }

    [HttpPost("{eventTypeId:guid}/versions/{eventTypeVersionId:guid}/retire")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionVersionDetailResponseDto>>> RetireVersion(
        Guid eventTypeId,
        Guid eventTypeVersionId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new RetireEventDefinitionVersionCommand(
                eventTypeId,
                eventTypeVersionId,
                _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<EventDefinitionVersionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost("{eventTypeId:guid}/retire")]
    [Authorize(Policy = PermissionCodes.EventDefinitions.Update)]
    public async Task<ActionResult<ApiResponseDto<EventDefinitionDetailResponseDto>>> Retire(
        Guid eventTypeId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new RetireEventDefinitionCommand(eventTypeId, _currentAdminContext.UserId),
            ct);

        return Ok(new ApiResponseDto<EventDefinitionDetailResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }
}
