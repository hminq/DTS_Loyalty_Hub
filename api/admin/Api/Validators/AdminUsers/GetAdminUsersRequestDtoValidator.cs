using Api.Dtos.Requests.AdminUsers;
using FluentValidation;

namespace Api.Validators.AdminUsers;

public sealed class GetAdminUsersRequestDtoValidator : AbstractValidator<GetAdminUsersRequestDto>
{
    public GetAdminUsersRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.Keyword)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");

        RuleFor(request => request.Status)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(25)
            .WithErrorCode("STATUS_TOO_LONG")
            .When(request => request.Status is not null)
            .OverridePropertyName("status");
    }
}
