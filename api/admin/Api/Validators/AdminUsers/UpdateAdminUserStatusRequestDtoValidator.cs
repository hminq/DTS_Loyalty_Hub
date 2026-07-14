using Api.Dtos.Requests.AdminUsers;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.AdminUsers;

public sealed class UpdateAdminUserStatusRequestDtoValidator
    : AbstractValidator<UpdateAdminUserStatusRequestDto>
{
    public UpdateAdminUserStatusRequestDtoValidator()
    {
        RuleFor(request => request.Status)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("STATUS_REQUIRED")
            .WithMessage("Status is required.")
            .Must(status => status is not null &&
                (string.Equals(status.Trim(), UserStatus.Enable, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(status.Trim(), UserStatus.Disable, StringComparison.OrdinalIgnoreCase)))
            .WithErrorCode("STATUS_INVALID")
            .WithMessage("Status is invalid.")
            .OverridePropertyName("status");
    }
}
