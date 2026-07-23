using Api.Dtos.Requests.CustomerUsers;
using FluentValidation;

namespace Api.Validators.CustomerUsers;

public sealed class GetCustomerUserHistoryRequestDtoValidator
    : AbstractValidator<GetCustomerUserHistoryRequestDto>
{
    public GetCustomerUserHistoryRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");
    }
}
