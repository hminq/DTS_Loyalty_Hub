using Core.UseCases.Campaigns.Results;

namespace Core.Abstractions;

public interface ICampaignEventDefinitionRepository
{
    Task<CampaignEventDefinitionResult?> GetByVersionIdAsync(
        Guid eventTypeVersionId,
        CancellationToken ct = default);

    Task<CampaignEventDefinitionResult?> GetForCampaignWriteAsync(
        Guid eventTypeVersionId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<CampaignEventDefinitionResult>> GetSelectableVersionsAsync(
        CancellationToken ct = default);
}
