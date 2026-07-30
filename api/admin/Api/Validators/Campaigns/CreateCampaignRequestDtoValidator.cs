using Api.Dtos.Requests.Campaigns;
using FluentValidation;
using FluentValidation.Results;

namespace Api.Validators.Campaigns;

public sealed class CreateCampaignRequestDtoValidator
    : AbstractValidator<CreateCampaignRequestDto>
{
    public CreateCampaignRequestDtoValidator()
    {
        CampaignWriteValidationRules.Apply(this);
        var actionValidator = new CampaignActionWriteRequestDtoValidator();

        RuleFor(request => request.Actions)
            .NotNull()
            .WithErrorCode("CAMPAIGN_ACTIONS_REQUIRED")
            .OverridePropertyName("actions");

        RuleFor(request => request.Actions)
            .Must(actions => actions is { Count: > 0 })
            .When(request => request.Actions is not null)
            .WithErrorCode("CAMPAIGN_ACTIONS_REQUIRED")
            .OverridePropertyName("actions");

        RuleFor(request => request.Actions).Custom((actions, context) =>
        {
            if (actions is null)
            {
                return;
            }

            for (var index = 0; index < actions.Count; index++)
            {
                var action = actions.ElementAt(index);
                if (action is null)
                {
                    context.AddFailure(new ValidationFailure(
                        $"actions[{index}]",
                        "Campaign action configuration is invalid.")
                    {
                        ErrorCode = "CAMPAIGN_ACTION_CONFIG_INVALID"
                    });
                    continue;
                }

                var validationResult = actionValidator.Validate(action);
                foreach (var failure in validationResult.Errors)
                {
                    context.AddFailure(new ValidationFailure(
                        $"actions[{index}].{failure.PropertyName}",
                        failure.ErrorMessage)
                    {
                        ErrorCode = failure.ErrorCode
                    });
                }
            }
        });
    }
}
