using Api.Dtos.Requests.Auth;
using FluentValidation;
using Api.Constants;

namespace Api.Validators;

public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty()
            .WithErrorCode("USERNAME_REQUIRED")
            .MaximumLength(ValidationConstants.MaxUsernameLength)
            .WithErrorCode("USERNAME_TOO_LONG")
            .OverridePropertyName("username");;

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .MaximumLength(ValidationConstants.MaxPasswordLength)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .OverridePropertyName("password");
    }
}
