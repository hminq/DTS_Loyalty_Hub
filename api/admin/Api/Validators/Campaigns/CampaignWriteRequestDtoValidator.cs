using Api.Dtos.Requests.Campaigns;
using FluentValidation;

namespace Api.Validators.Campaigns;

public sealed class CampaignWriteRequestDtoValidator
    : AbstractValidator<CampaignWriteRequestDto>
{
    public CampaignWriteRequestDtoValidator()
    {
        CampaignWriteValidationRules.Apply(this);
    }
}
