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
            .MaximumLength(50)
            .WithErrorCode("USERNAME_TOO_LONG")
            .OverridePropertyName("username");

        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress()
            .WithErrorCode("EMAIL_INVALID")
            .MaximumLength(50)
            .WithErrorCode("EMAIL_TOO_LONG")
            .OverridePropertyName("email");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("PASSWORD_REQUIRED")
            .MinimumLength(8)
            .WithErrorCode("PASSWORD_TOO_SHORT")
            .MaximumLength(100)
            .WithErrorCode("PASSWORD_TOO_LONG")
            .OverridePropertyName("password");

        RuleFor(request => request.FullName)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50)
            .WithErrorCode("FULL_NAME_TOO_LONG")
            .When(request => request.FullName is not null)
            .OverridePropertyName("fullName");

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(15)
            .WithErrorCode("PHONE_NUMBER_TOO_LONG")
            .When(request => request.PhoneNumber is not null)
            .OverridePropertyName("phoneNumber");

        RuleFor(request => request.RoleId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("ROLE_ID_REQUIRED")
            .OverridePropertyName("roleId");
    }
}
