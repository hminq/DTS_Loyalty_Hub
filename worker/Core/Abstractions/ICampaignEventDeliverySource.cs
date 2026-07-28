using Core.Entities.Campaigns;

namespace Core.Abstractions;

public interface ICampaignEventDeliverySource
{
    Task RunAsync(
        Func<
            CampaignEventDelivery,
            CancellationToken,
            Task<CampaignEventDeliveryResult>> deliveryHandler,
        CancellationToken cancellationToken);
}
