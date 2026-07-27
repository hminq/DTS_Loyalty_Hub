using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class GetCampaignByIdQueryHandler
    : IRequestHandler<GetCampaignByIdQuery, CampaignDetailResult>
{
    private const int SessionPreviewLimit = 100;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IBannerReadUrlProvider _bannerReadUrlProvider;

    public GetCampaignByIdQueryHandler(
        ICampaignRepository campaignRepository,
        IBannerReadUrlProvider bannerReadUrlProvider)
    {
        _campaignRepository = campaignRepository;
        _bannerReadUrlProvider = bannerReadUrlProvider;
    }

    public async Task<CampaignDetailResult> Handle(
        GetCampaignByIdQuery request,
        CancellationToken ct)
    {
        if (request.CampaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        var result = await _campaignRepository.GetByIdAsync(
                request.CampaignId,
                SessionPreviewLimit,
                ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(result.BannerImageKey))
        {
            return result;
        }

        return result with
        {
            BannerImageUrl = _bannerReadUrlProvider.CreateReadUrl(result.BannerImageKey)
        };
    }
}
