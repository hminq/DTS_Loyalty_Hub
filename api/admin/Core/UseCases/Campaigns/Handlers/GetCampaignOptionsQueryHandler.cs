using Core.Abstractions;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class GetCampaignOptionsQueryHandler
    : IRequestHandler<GetCampaignOptionsQuery, CampaignOptionsResult>
{
    private readonly ICampaignConfigurationService _configurationService;

    public GetCampaignOptionsQueryHandler(ICampaignConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public Task<CampaignOptionsResult> Handle(
        GetCampaignOptionsQuery request,
        CancellationToken ct)
    {
        return Task.FromResult(_configurationService.GetOptions());
    }
}
