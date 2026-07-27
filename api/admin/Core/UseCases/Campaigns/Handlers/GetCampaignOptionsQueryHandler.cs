using Campaign.Contracts.Constants;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using MediatR;
using Messaging.Contracts.Events;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class GetCampaignOptionsQueryHandler
    : IRequestHandler<GetCampaignOptionsQuery, CampaignOptionsResult>
{
    public Task<CampaignOptionsResult> Handle(
        GetCampaignOptionsQuery request,
        CancellationToken ct)
    {
        var issuePoint = new CampaignActionTypeOptionResult(
            ActionTypes.IssuePoint,
            [
                new CampaignCapabilityOptionResult(PointCalculationTypes.FixedAmount, true),
                new CampaignCapabilityOptionResult(PointCalculationTypes.Percent, false)
            ],
            [
                new CampaignCapabilityOptionResult(PointRecipients.EventCustomer, true),
                new CampaignCapabilityOptionResult(PointRecipients.Referrer, true)
            ]);

        var result = new CampaignOptionsResult(
            CampaignStatuses.All,
            new CampaignScheduleOptionsResult(
                CampaignScheduleDefaults.TimeZone,
                CampaignScheduleDays.CanonicalOrder),
            [
                new CampaignEventTypeOptionResult(
                    EventTypeCodes.CustomerAccountRegistered,
                    new CampaignConditionOptionsResult(
                    [
                        new CampaignConditionOptionResult(
                            CustomerRegistrationConditionOptionCodes.AllRegistrations,
                            [],
                            true),
                        new CampaignConditionOptionResult(
                            CustomerRegistrationConditionOptionCodes.NormalRegistration,
                            [CustomerRegistrationSources.Normal],
                            true),
                        new CampaignConditionOptionResult(
                            CustomerRegistrationConditionOptionCodes.ReferralRegistration,
                            [CustomerRegistrationSources.Referral],
                            true)
                    ]),
                    [ActionTypes.IssuePoint])
            ],
            [issuePoint]);

        return Task.FromResult(result);
    }
}
