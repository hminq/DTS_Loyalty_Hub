using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface ICampaignEventDeliverySource
{
    Task RunAsync(
        Func<
            CampaignEventDelivery,
            CancellationToken,
            Task<CampaignEventDeliveryResult>> deliveryHandler,
        CancellationToken cancellationToken);
}
