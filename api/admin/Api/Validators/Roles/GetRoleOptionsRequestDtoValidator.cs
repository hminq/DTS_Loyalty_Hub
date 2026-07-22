using Api.Dtos.Requests.Roles;
using FluentValidation;

namespace Api.Validators.Roles;

public sealed class GetRoleOptionsRequestDtoValidator : AbstractValidator<GetRoleOptionsRequestDto>
{
    public GetRoleOptionsRequestDtoValidator()
    {
        RuleFor(request => request.Keyword)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");
    }
}
