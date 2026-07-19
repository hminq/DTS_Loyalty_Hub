using Api.Dtos.Requests.VoucherDefinitions;
using FluentValidation;

namespace Api.Validators.VoucherDefinitions;

public sealed class GetVoucherDefinitionsRequestDtoValidator
    : AbstractValidator<GetVoucherDefinitionsRequestDto>
{
    public GetVoucherDefinitionsRequestDtoValidator()
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
    }
}
