using Api.Dtos.Requests.AdminUsers;
using FluentValidation;

namespace Api.Validators.AdminUsers;

public sealed class CreateAdminUserRequestDtoValidator : AbstractValidator<CreateAdminUserRequestDto>
{
    public CreateAdminUserRequestDtoValidator()
    {
        RuleFor(request => request.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("USERNAME_REQUIRED")
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithErrorCode("USERNAME_TOO_LONG")
            .WithMessage("Username must be 50 characters or fewer.")
            .OverridePropertyName("username");

        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("EMAIL_REQUIRED")
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode("EMAIL_INVALID")
            .WithMessage("Email is invalid.")
            .MaximumLength(50)
            .WithErrorCode("EMAIL_TOO_LONG")
            .WithMessage("Email must be 50 characters or fewer.")
            .OverridePropertyName("email");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithErrorCode("PASSWORD_TOO_SHORT")
            .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .WithMessage("Password must be 100 characters or fewer.")
            .OverridePropertyName("password");

        RuleFor(request => request.FullName)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50)
            .WithErrorCode("FULL_NAME_TOO_LONG")
            .WithMessage("Full name must be 50 characters or fewer.")
            .When(request => request.FullName is not null)
            .OverridePropertyName("fullName");

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(15)
            .WithErrorCode("PHONE_NUMBER_TOO_LONG")
            .WithMessage("Phone number must be 15 characters or fewer.")
            .When(request => request.PhoneNumber is not null)
            .OverridePropertyName("phoneNumber");

        RuleFor(request => request.RoleId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("ROLE_ID_REQUIRED")
            .WithMessage("Role id is required.")
            .OverridePropertyName("roleId");
    }
}
