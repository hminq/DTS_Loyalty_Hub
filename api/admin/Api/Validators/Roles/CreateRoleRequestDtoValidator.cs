using Api.Dtos.Requests.Roles;
using FluentValidation;

namespace Api.Validators.Roles;

public sealed class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestDtoValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithErrorCode("ROLE_NAME_REQUIRED")
            .WithMessage("Role name is required.")
            .MaximumLength(100)
            .WithErrorCode("ROLE_NAME_TOO_LONG")
            .WithMessage("Role name must be 100 characters or fewer.")
            .OverridePropertyName("name");

        RuleFor(request => request.PermissionIds)
            .NotEmpty()
            .WithErrorCode("ROLE_PERMISSION_IDS_REQUIRED")
            .WithMessage("Permission ids are required.")
            .Must(permissionIds => permissionIds.Distinct().Count() == permissionIds.Count)
            .WithErrorCode("ROLE_PERMISSION_DUPLICATED")
            .WithMessage("Permission ids must be unique.")
            .OverridePropertyName("permissionIds");

        RuleForEach(request => request.PermissionIds)
            .NotEmpty()
            .WithErrorCode("PERMISSION_ID_REQUIRED")
            .WithMessage("Permission id is required.");
    }
}
