using Api.Dtos.Requests.Auth;
using FluentValidation;

namespace Api.Validators.Auth;

public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty()
            .WithErrorCode("USERNAME_REQUIRED")
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithErrorCode("USERNAME_TOO_LONG")
            .WithMessage("Username must be 50 characters or fewer.")
            .OverridePropertyName("username");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .WithMessage("Password is required.")
            .MaximumLength(200)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .WithMessage("Password must be 200 characters or fewer.")
            .OverridePropertyName("password");
    }
}
