using Api.Dtos.Requests.Roles;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Roles;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Roles.Commands;
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
    private readonly IValidator<CreateRoleRequestDto> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequestDto> _updateRoleValidator;

    public RolesController(
        ISender sender,
        IValidator<CreateRoleRequestDto> createRoleValidator,
        IValidator<UpdateRoleRequestDto> updateRoleValidator)
    {
        _sender = sender;
        _createRoleValidator = createRoleValidator;
        _updateRoleValidator = updateRoleValidator;
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

        var result = await _sender.Send(request.ToCommand(), ct);
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

        var result = await _sender.Send(request.ToCommand(roleId), ct);

        return Ok(new ApiResponseDto<RoleResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpDelete("{roleId:guid}")]
    [Authorize(Policy = PermissionCodes.Roles.Delete)]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken ct)
    {
        await _sender.Send(new DeleteRoleCommand(roleId), ct);

        return NoContent();
    }
}
