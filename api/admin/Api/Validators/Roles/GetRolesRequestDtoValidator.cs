using Api.Dtos.Requests.Roles;
using FluentValidation;

namespace Api.Validators.Roles;

public sealed class GetRolesRequestDtoValidator : AbstractValidator<GetRolesRequestDto>
{
    public GetRolesRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .WithMessage("Page must be greater than or equal to 1.")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .WithMessage("Page size must be between 1 and 100.")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.Keyword)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .WithMessage("Keyword must be 100 characters or fewer.")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");
    }
}
