using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface ICampaignEventRuntimeDefinition
{
    string EventType { get; }

    IValidatedCampaignEvent ValidateDelivery(CampaignEventDelivery delivery);

    CampaignFactValue GetFact(
        IValidatedCampaignEvent campaignEvent,
        string fieldCode);

    CampaignTargetResolution ResolveTarget(
        IValidatedCampaignEvent campaignEvent,
        string selector);
}
