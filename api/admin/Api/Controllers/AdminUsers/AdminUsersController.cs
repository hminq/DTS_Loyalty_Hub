using Api.Authentication;
using Api.Dtos.Requests.AdminUsers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.AdminUsers;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AdminUsers;

[ApiController]
[Route("api/admin/admin-users")]
[Authorize]
public sealed class AdminUsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentAdminContext _currentAdminContext;
    private readonly IValidator<GetAdminUsersRequestDto> _getAdminUsersValidator;
    private readonly IValidator<CreateAdminUserRequestDto> _createAdminUserValidator;
    private readonly IValidator<UpdateAdminUserRequestDto> _updateAdminUserValidator;
    private readonly IValidator<UpdateAdminUserStatusRequestDto> _updateAdminUserStatusValidator;

    public AdminUsersController(
        ISender sender,
        ICurrentAdminContext currentAdminContext,
        IValidator<GetAdminUsersRequestDto> getAdminUsersValidator,
        IValidator<CreateAdminUserRequestDto> createAdminUserValidator,
        IValidator<UpdateAdminUserRequestDto> updateAdminUserValidator,
        IValidator<UpdateAdminUserStatusRequestDto> updateAdminUserStatusValidator)
    {
        _sender = sender;
        _currentAdminContext = currentAdminContext;
        _getAdminUsersValidator = getAdminUsersValidator;
        _createAdminUserValidator = createAdminUserValidator;
        _updateAdminUserValidator = updateAdminUserValidator;
        _updateAdminUserStatusValidator = updateAdminUserStatusValidator;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.AdminUsers.View)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<AdminUserListItemResponseDto>>>> GetList(
        [FromQuery] GetAdminUsersRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _getAdminUsersValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToQuery(), ct);

        return Ok(result.ToPagedResponseDto());
    }

    [HttpGet("{adminId:guid}")]
    [Authorize(Policy = PermissionCodes.AdminUsers.View)]
    public async Task<ActionResult<ApiResponseDto<AdminUserResponseDto>>> GetById(
        Guid adminId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetAdminUserByIdQuery(adminId), ct);

        return Ok(new ApiResponseDto<AdminUserResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.AdminUsers.Create)]
    public async Task<ActionResult<ApiResponseDto<AdminUserResponseDto>>> Create(
        [FromBody] CreateAdminUserRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _createAdminUserValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToCommand(_currentAdminContext.UserId), ct);
        var response = new ApiResponseDto<AdminUserResponseDto>
        {
            Data = result.ToResponseDto()
        };

        return Created($"/api/admin/admin-users/{response.Data.AdminId}", response);
    }

    [HttpPut("{adminId:guid}")]
    [Authorize(Policy = PermissionCodes.AdminUsers.Update)]
    public async Task<ActionResult<ApiResponseDto<AdminUserResponseDto>>> Update(
        Guid adminId,
        [FromBody] UpdateAdminUserRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _updateAdminUserValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        var result = await _sender.Send(request.ToCommand(adminId, _currentAdminContext.UserId), ct);

        return Ok(new ApiResponseDto<AdminUserResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [HttpPatch("{adminId:guid}/status")]
    [Authorize(Policy = PermissionCodes.AdminUsers.Disable)]
    public async Task<IActionResult> UpdateStatus(
        Guid adminId,
        [FromBody] UpdateAdminUserStatusRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _updateAdminUserStatusValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponseDto.Validation(
                ValidationErrorMapper.FromValidationFailures(validationResult.Errors)));
        }

        await _sender.Send(request.ToCommand(adminId, _currentAdminContext.UserId), ct);

        return NoContent();
    }

    [HttpPost("{adminId:guid}/revoke-session")]
    [Authorize(Policy = PermissionCodes.AdminUsers.RevokeSession)]
    public async Task<IActionResult> RevokeSession(Guid adminId, CancellationToken ct)
    {
        await _sender.Send(new RevokeAdminSessionCommand(adminId, _currentAdminContext.UserId), ct);

        return NoContent();
    }

}
