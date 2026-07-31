using Api.Dtos.Requests.DevelopmentEvents;
using FluentValidation;

namespace Api.Validators.DevelopmentEvents;

public sealed class CustomerMissionCompletedRequestDtoValidator
    : AbstractValidator<CustomerMissionCompletedRequestDto>
{
    public CustomerMissionCompletedRequestDtoValidator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty()
            .WithErrorCode("CUSTOMER_ID_REQUIRED")
            .OverridePropertyName("customerId");

        RuleFor(request => request.MissionCode)
            .NotEmpty()
            .WithErrorCode("VALUE_INVALID")
            .OverridePropertyName("missionCode");

        RuleFor(request => request.MissionCategory)
            .NotEmpty()
            .WithErrorCode("VALUE_INVALID")
            .OverridePropertyName("missionCategory");

        RuleFor(request => request.IsFirstCompletion)
            .NotNull()
            .WithErrorCode("VALUE_INVALID")
            .OverridePropertyName("isFirstCompletion");

        RuleFor(request => request.CompletionCount)
            .GreaterThan(0)
            .WithErrorCode("VALUE_INVALID")
            .OverridePropertyName("completionCount");
    }
}
