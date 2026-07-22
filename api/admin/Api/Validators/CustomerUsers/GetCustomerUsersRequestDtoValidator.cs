using Api.Dtos.Requests.CustomerUsers;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.CustomerUsers;

public sealed class GetCustomerUsersRequestDtoValidator
    : AbstractValidator<GetCustomerUsersRequestDto>
{
    public GetCustomerUsersRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.Keyword)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");

        RuleFor(request => request.Status)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(25)
            .WithErrorCode("STATUS_TOO_LONG")
            .Must(status =>
                string.Equals(status?.Trim(), UserStatus.Enable, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status?.Trim(), UserStatus.Disable, StringComparison.OrdinalIgnoreCase))
            .WithErrorCode("STATUS_INVALID")
            .When(request => request.Status is not null)
            .OverridePropertyName("status");

        RuleFor(request => request.TierId)
            .Must(tierId => !tierId.HasValue || tierId.Value != Guid.Empty)
            .WithErrorCode("TIER_ID_INVALID")
            .OverridePropertyName("tierId");
    }
}
