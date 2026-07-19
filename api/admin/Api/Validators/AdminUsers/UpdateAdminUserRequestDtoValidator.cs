using Api.Dtos.Requests.AdminUsers;
using FluentValidation;

namespace Api.Validators.AdminUsers;

public sealed class UpdateAdminUserRequestDtoValidator : AbstractValidator<UpdateAdminUserRequestDto>
{
    public UpdateAdminUserRequestDtoValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress()
            .WithErrorCode("EMAIL_INVALID")
            .MaximumLength(50)
            .WithErrorCode("EMAIL_TOO_LONG")
            .OverridePropertyName("email");

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
