using Api.Dtos.Requests.CustomerUsers;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.CustomerUsers;

public sealed class UpdateCustomerUserStatusRequestDtoValidator
    : AbstractValidator<UpdateCustomerUserStatusRequestDto>
{
    public UpdateCustomerUserStatusRequestDtoValidator()
    {
        RuleFor(request => request.Status)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("STATUS_REQUIRED")
            .Must(status => status is not null &&
                (string.Equals(status.Trim(), UserStatus.Enable, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(status.Trim(), UserStatus.Disable, StringComparison.OrdinalIgnoreCase)))
            .WithErrorCode("STATUS_INVALID")
            .OverridePropertyName("status");
    }
}
