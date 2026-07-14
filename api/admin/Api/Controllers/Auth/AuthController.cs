using Api.Dtos.Requests.Auth;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Auth;
using Api.Mappers;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth;

[ApiController]
[Route("api/admin/")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<LoginRequestDto> _loginRequestValidator;

    public AuthController(
        ISender sender,
        IValidator<LoginRequestDto> loginRequestValidator)
    {
        _sender = sender;
        _loginRequestValidator = loginRequestValidator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _loginRequestValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            var details = ValidationErrorMapper.FromValidationFailures(validationResult.Errors);

            return BadRequest(ApiErrorResponseDto.Validation(details));
        }

        var result = await _sender.Send(
            request.ToCommand(),
            ct);

        return Ok(new ApiResponseDto<LoginResponseDto>
        {
            Data = result.ToResponseDto()
        });
    }
}
