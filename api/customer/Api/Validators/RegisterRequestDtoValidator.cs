using Api.Dtos.Requests.Auth;
using Api.Constants;
using FluentValidation;

namespace Api.Validators;

public sealed class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Username)
            .NotEmpty()
            .WithErrorCode("USERNAME_REQUIRED")
            .Length(ValidationConstants.MinUsernameLength, ValidationConstants.MaxUsernameLength)
            .WithErrorCode("USERNAME_INVALID_LENGTH")
            .OverridePropertyName("username");

        RuleFor(request => request.Email)
            .NotEmpty()
            .WithErrorCode("EMAIL_REQUIRED")
            .Length(ValidationConstants.MinEmailLength, ValidationConstants.MaxEmailLength)
            .WithErrorCode("EMAIL_INVALID_LENGTH")
            .EmailAddress()
            .WithErrorCode("EMAIL_INVALID_FORMAT")
            .OverridePropertyName("email");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .MinimumLength(ValidationConstants.MinPasswordLength)
            .WithErrorCode("PASSWORD_TOO_SHORT")
            .MaximumLength(ValidationConstants.MaxPasswordLength)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .Matches("[A-Z]")
            .WithErrorCode("PASSWORD_MISSING_UPPERCASE")
            .Matches("[0-9]")
            .WithErrorCode("PASSWORD_MISSING_DIGIT")
            .OverridePropertyName("password");

        RuleFor(request => request.FullName)
            .NotEmpty()
            .WithErrorCode("FULLNAME_REQUIRED")
            .Length(ValidationConstants.MinFullNameLength, ValidationConstants.MaxFullNameLength)
            .WithErrorCode("FULLNAME_INVALID_LENGTH")
            .Matches(ValidationConstants.FullNamePattern)
            .WithErrorCode("FULLNAME_INVALID_FORMAT")
            .OverridePropertyName("fullName");

        RuleFor(request => request.Phone)
            .NotEmpty()
            .WithErrorCode("PHONE_REQUIRED")
            .Matches(ValidationConstants.PhonePattern)
            .WithErrorCode("PHONE_INVALID_FORMAT")
            .OverridePropertyName("phone");
    }
}
