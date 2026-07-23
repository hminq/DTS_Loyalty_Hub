using Api.Dtos.Requests.AdminUsers;
using Core.Entities;
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
            .MinimumLength(UserProfileRules.MinEmailLength)
            .WithErrorCode("EMAIL_INVALID")
            .MaximumLength(UserProfileRules.MaxEmailLength)
            .WithErrorCode("EMAIL_TOO_LONG")
            .EmailAddress()
            .WithErrorCode("EMAIL_INVALID")
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
            .MinimumLength(UserProfileRules.MinFullNameLength)
            .WithErrorCode("FULL_NAME_TOO_SHORT")
            .MaximumLength(UserProfileRules.MaxFullNameLength)
            .WithErrorCode("FULL_NAME_TOO_LONG")
            .Matches(UserProfileRules.FullNamePattern)
            .WithErrorCode("FULL_NAME_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.FullName))
            .OverridePropertyName("fullName");

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(UserProfileRules.MaxPhoneNumberLength)
            .WithErrorCode("PHONE_NUMBER_TOO_LONG")
            .Matches(UserProfileRules.PhoneNumberPattern)
            .WithErrorCode("PHONE_NUMBER_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.PhoneNumber))
            .OverridePropertyName("phoneNumber");

        RuleFor(request => request.RoleId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("ROLE_ID_REQUIRED")
            .OverridePropertyName("roleId");
    }
}
