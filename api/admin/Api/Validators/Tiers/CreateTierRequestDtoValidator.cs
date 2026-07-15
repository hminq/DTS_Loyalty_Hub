using Api.Dtos.Requests.Tiers;
using FluentValidation;

namespace Api.Validators.Tiers;

public sealed class CreateTierRequestDtoValidator : AbstractValidator<CreateTierRequestDto>
{
    
    public CreateTierRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;


        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("TIER_NAME_REQUIRED")
            .WithMessage("Tier name is required.")
            .MinimumLength(3)
            .WithErrorCode("TIER_NAME_LENGTH_INVALID")
            .WithMessage("Tier name must be between 3 and 49 characters.")
            .MaximumLength(49)
            .WithErrorCode("TIER_NAME_LENGTH_INVALID")
            .WithMessage("Tier name must be between 3 and 49 characters.")
            .OverridePropertyName("name");

        RuleFor(request => request.CycleMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode("TIER_CYCLE_MONTH_INVALID")
            .WithMessage("Cycle month must be between 1 and 12.")
            .OverridePropertyName("cycleMonth");

        RuleFor(request => request.PointsRequired)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("TIER_POINTS_REQUIRED_INVALID")
            .WithMessage("Points required cannot be negative.")
            .OverridePropertyName("pointsRequired");

        RuleFor(request => request.Priority)
            .GreaterThan(0)
            .WithErrorCode("TIER_PRIORITY_INVALID")
            .WithMessage("Priority must be greater than zero.")
            .OverridePropertyName("priority");
    }
}
