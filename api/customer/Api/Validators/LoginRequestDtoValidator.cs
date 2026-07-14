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
            .WithMessage("Username is required.")
            .MaximumLength(ValidationConstants.MaxUsernameLength)
            .WithErrorCode("USERNAME_TOO_LONG")
            .WithMessage("Username must be {MaximumLength} characters or fewer.")
            .OverridePropertyName("username");;

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .WithMessage("Password is required.")
            .MaximumLength(ValidationConstants.MaxPasswordLength)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .WithMessage("Password must be {MaximumLength} characters or fewer.")
            .OverridePropertyName("password");
    }
}
