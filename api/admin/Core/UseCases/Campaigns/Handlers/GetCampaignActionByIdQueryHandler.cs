using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class GetCampaignActionByIdQueryHandler
    : IRequestHandler<GetCampaignActionByIdQuery, CampaignActionResult>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetCampaignActionByIdQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignActionResult> Handle(
        GetCampaignActionByIdQuery request,
        CancellationToken ct)
    {
        if (request.CampaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        if (request.ActionId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ACTION_ID_REQUIRED", DomainErrorType.Validation);
        }

        return await _campaignRepository.GetActionByIdAsync(
                request.CampaignId,
                request.ActionId,
                ct)
            ?? throw new DomainException("CAMPAIGN_ACTION_NOT_FOUND", DomainErrorType.NotFound);
    }
}
