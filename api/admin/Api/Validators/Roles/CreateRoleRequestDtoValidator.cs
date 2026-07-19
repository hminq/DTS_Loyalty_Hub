using Api.Dtos.Requests.Roles;
using FluentValidation;

namespace Api.Validators.Roles;

public sealed class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestDtoValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("ROLE_NAME_REQUIRED")
            .MaximumLength(100)
            .WithErrorCode("ROLE_NAME_TOO_LONG")
            .OverridePropertyName("name");

        RuleFor(request => request.PermissionIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("ROLE_PERMISSION_IDS_REQUIRED")
            .Must(permissionIds => permissionIds.Distinct().Count() == permissionIds.Count)
            .WithErrorCode("ROLE_PERMISSION_DUPLICATED")
            .OverridePropertyName("permissionIds");

        RuleForEach(request => request.PermissionIds)
            .NotEmpty()
            .WithErrorCode("PERMISSION_ID_REQUIRED");
    }
}
