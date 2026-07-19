using Api.Dtos.Requests.Notifications;
using FluentValidation;

namespace Api.Validators.Notifications;

public sealed class GetEventTypesRequestDtoValidator : AbstractValidator<GetEventTypesRequestDto>
{
    public GetEventTypesRequestDtoValidator()
    {
        RuleFor(request => request.SearchKeyword)
            .MaximumLength(100)
            .WithErrorCode("SEARCH_KEYWORD_TOO_LONG")
            .When(x => !string.IsNullOrEmpty(x.SearchKeyword))
            .OverridePropertyName("searchKeyword");
    }
}
