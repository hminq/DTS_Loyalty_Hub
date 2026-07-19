using Api.Dtos.Requests.Auth;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Auth;
using Api.Mappers;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth;

[ApiController]
[Route("api/users/")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<LoginRequestDto> _loginRequestValidator;
    private readonly IValidator<RegisterRequestDto> _registerRequestValidator;
    private readonly ValidationErrorMapper _validationErrorMapper;

    public AuthController(
        ISender sender,
        IValidator<LoginRequestDto> loginRequestValidator,
        IValidator<RegisterRequestDto> registerRequestValidator,
        ValidationErrorMapper validationErrorMapper)
    {
        _sender = sender;
        _loginRequestValidator = loginRequestValidator;
        _registerRequestValidator = registerRequestValidator;
        _validationErrorMapper = validationErrorMapper;
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

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseDto<RegisterResponseDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _registerRequestValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            return BadRequest(_validationErrorMapper.FromValidationFailures(validationResult.Errors));
        }

        var result = await _sender.Send(
            request.ToCommand(),
            ct);

        return StatusCode(StatusCodes.Status201Created, new ApiResponseDto<RegisterResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }
}
