using Api.Dtos.Requests.Auth;
using FluentValidation;

namespace Api.Validators.Auth;

public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(request => request.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("USERNAME_REQUIRED")
            .MaximumLength(50)
            .WithErrorCode("USERNAME_TOO_LONG")
            .OverridePropertyName("username");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .MaximumLength(200)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .OverridePropertyName("password");
    }
}
