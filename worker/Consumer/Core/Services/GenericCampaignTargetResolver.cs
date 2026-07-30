using Campaign.Contracts.Constants;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;

namespace Consumer.Core.Services;

public sealed class GenericCampaignTargetResolver
{
    public CampaignTargetResolution Resolve(
        GenericValidatedCampaignEvent campaignEvent,
        string selector,
        string requiredTargetKind)
    {
        ArgumentNullException.ThrowIfNull(campaignEvent);

        if (!campaignEvent.Definition.TargetsBySelector.TryGetValue(selector, out var target))
        {
            return InvalidConfiguration();
        }

        if (!string.Equals(target.Kind, CampaignTargetKinds.Customer, StringComparison.Ordinal) ||
            !string.Equals(target.Kind, requiredTargetKind, StringComparison.Ordinal))
        {
            return InvalidConfiguration(target.Kind);
        }

        if (!campaignEvent.PayloadValues.TryGetValue(target.IdField, out var value) ||
            value.Value is not string rawId ||
            !Guid.TryParse(rawId, out var targetId) ||
            targetId == Guid.Empty)
        {
            return new CampaignTargetResolution(
                CampaignTargetResolutionStatuses.InvalidEvent,
                target.Kind,
                null,
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        return new CampaignTargetResolution(
            CampaignTargetResolutionStatuses.Resolved,
            target.Kind,
            targetId,
            null);
    }

    private static CampaignTargetResolution InvalidConfiguration(string targetKind = "")
    {
        return new CampaignTargetResolution(
            CampaignTargetResolutionStatuses.Unsupported,
            targetKind,
            null,
            CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }
}
