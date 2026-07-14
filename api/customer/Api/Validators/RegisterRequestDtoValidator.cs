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
            .WithMessage("Username is required.")
            .Length(ValidationConstants.MinUsernameLength, ValidationConstants.MaxUsernameLength)
            .WithErrorCode("USERNAME_INVALID_LENGTH")
            .WithMessage("Username must be between {MinimumLength} and {MaximumLength} characters.")
            .OverridePropertyName("username");

        RuleFor(request => request.Email)
            .NotEmpty()
            .WithErrorCode("EMAIL_REQUIRED")
            .WithMessage("Email is required.")
            .Length(ValidationConstants.MinEmailLength, ValidationConstants.MaxEmailLength)
            .WithErrorCode("EMAIL_INVALID_LENGTH")
            .WithMessage("Email must be between 5 and 50 characters.")
            .EmailAddress()
            .WithErrorCode("EMAIL_INVALID_FORMAT")
            .WithMessage("Email format is invalid.")
            .OverridePropertyName("email");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .WithMessage("Password is required.")
            .MinimumLength(ValidationConstants.MinPasswordLength)
            .WithErrorCode("PASSWORD_TOO_SHORT")
            .WithMessage("Password must be at least {MinimumLength} characters.")
            .MaximumLength(ValidationConstants.MaxPasswordLength)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .WithMessage("Password must be {MaximumLength} characters or fewer.")
            .Matches("[A-Z]")
            .WithErrorCode("PASSWORD_MISSING_UPPERCASE")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]")
            .WithErrorCode("PASSWORD_MISSING_DIGIT")
            .WithMessage("Password must contain at least one digit.")
            .OverridePropertyName("password");

        RuleFor(request => request.FullName)
            .NotEmpty()
            .WithErrorCode("FULLNAME_REQUIRED")
            .WithMessage("Full name is required.")
            .Length(ValidationConstants.MinFullNameLength, ValidationConstants.MaxFullNameLength)
            .WithErrorCode("FULLNAME_INVALID_LENGTH")
            .WithMessage("Full name must be between {MinimumLength} and {MaximumLength} characters.")
            .OverridePropertyName("fullName");

        RuleFor(request => request.Phone)
            .NotEmpty()
            .WithErrorCode("PHONE_REQUIRED")
            .WithMessage("Phone is required.")
            .Matches(ValidationConstants.PhonePattern)
            .WithErrorCode("PHONE_INVALID_FORMAT")
            .WithMessage("Phone format is invalid.")
            .OverridePropertyName("phone");
    }
}
