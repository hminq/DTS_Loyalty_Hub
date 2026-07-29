using Core.Abstractions;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class GetCampaignOptionsQueryHandler
    : IRequestHandler<GetCampaignOptionsQuery, CampaignOptionsResult>
{
    private readonly ICampaignConfigurationService _configurationService;
    private readonly ICampaignEventDefinitionRepository _eventDefinitionRepository;

    public GetCampaignOptionsQueryHandler(
        ICampaignConfigurationService configurationService,
        ICampaignEventDefinitionRepository eventDefinitionRepository)
    {
        _configurationService = configurationService;
        _eventDefinitionRepository = eventDefinitionRepository;
    }

    public async Task<CampaignOptionsResult> Handle(
        GetCampaignOptionsQuery request,
        CancellationToken ct)
    {
        var eventDefinitions = await _eventDefinitionRepository.GetSelectableVersionsAsync(ct);
        return _configurationService.BuildOptions(eventDefinitions);
    }
}
