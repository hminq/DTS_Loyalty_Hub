using Api.Authentication;
using Api.Dtos.Requests.Roles;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Roles;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Roles;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<GetRolesRequestDto> _getRolesValidator;
    private readonly IValidator<CreateRoleRequestDto> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequestDto> _updateRoleValidator;

    public RolesController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetRolesRequestDto> getRolesValidator,
        IValidator<CreateRoleRequestDto> createRoleValidator,
        IValidator<UpdateRoleRequestDto> updateRoleValidator)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getRolesValidator = getRolesValidator;
        _createRoleValidator = createRoleValidator;
        _updateRoleValidator = updateRoleValidator;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.Roles.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<RoleResponseDto>>>> GetList(
        [FromQuery] GetRolesRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getRolesValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("{roleId:guid}")]
    [Authorize(Policy = PermissionCodes.Roles.View)]
    public async Task<ActionResult<ApiResponseDto<RoleResponseDto>>> GetById(
        Guid roleId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetRoleByIdQuery(roleId), ct);

        return Ok(new ApiResponseDto<RoleResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.Roles.Create)]
    public async Task<ActionResult<ApiResponseDto<RoleResponseDto>>> Create(
        [FromBody] CreateRoleRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _createRoleValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToCommand(_currentAdminContext.UserId), ct);
        var response = new ApiResponseDto<RoleResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return Created($"/api/admin/roles/{response.Data.RoleId}", response);
    }

    [HttpPut("{roleId:guid}")]
    [Authorize(Policy = PermissionCodes.Roles.Update)]
    public async Task<ActionResult<ApiResponseDto<RoleResponseDto>>> Update(
        Guid roleId,
        [FromBody] UpdateRoleRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _updateRoleValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToCommand(roleId, _currentAdminContext.UserId), ct);

        return Ok(new ApiResponseDto<RoleResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpDelete("{roleId:guid}")]
    [Authorize(Policy = PermissionCodes.Roles.Delete)]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken ct)
    {
        await _sender.Send(new DeleteRoleCommand(roleId, _currentAdminContext.UserId), ct);

        return NoContent();
    }

}
