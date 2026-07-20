using Api.Authentication;
using Api.Dtos.Requests.Auth;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Auth;
using Api.Mappers;
using Core.UseCases.Auth.Commands;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth;

[ApiController]
[Route("api/admin/")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<LoginRequestDto> _loginRequestValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;
    private readonly IAuthenticatedAdminSessionAccessor _adminSessionAccessor;

    public AuthController(
        ISender sender,
        IValidator<LoginRequestDto> loginRequestValidator,
        ValidationErrorMapper validationErrorMapper,
        IAuthenticatedAdminSessionAccessor adminSessionAccessor)
    {
        _sender = sender;
        _loginRequestValidator = loginRequestValidator;
        _validationErrorMapper = validationErrorMapper;
        _adminSessionAccessor = adminSessionAccessor;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _loginRequestValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToCommand(),
            ct);

        return Ok(new ApiResponseDto<LoginResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!_adminSessionAccessor.TryGet(out var session))
        {
            return Unauthorized();
        }

        await _sender.Send(
            new LogoutCommand(
                session.AdminId,
                session.AdminSessionId,
                session.AccessTokenJti),
            ct);

        return NoContent();
    }
}
