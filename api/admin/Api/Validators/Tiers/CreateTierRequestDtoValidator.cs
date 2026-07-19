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
            .MinimumLength(3)
            .WithErrorCode("TIER_NAME_LENGTH_INVALID")
            .MaximumLength(49)
            .WithErrorCode("TIER_NAME_LENGTH_INVALID")
            .OverridePropertyName("name");

        RuleFor(request => request.CycleMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode("TIER_CYCLE_MONTH_INVALID")
            .OverridePropertyName("cycleMonth");

        RuleFor(request => request.PointsRequired)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("TIER_POINTS_REQUIRED_INVALID")
            .OverridePropertyName("pointsRequired");

        RuleFor(request => request.Priority)
            .GreaterThan(0)
            .WithErrorCode("TIER_PRIORITY_INVALID")
            .OverridePropertyName("priority");
    }
}
