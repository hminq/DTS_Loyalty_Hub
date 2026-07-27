using Campaign.Contracts.Constants;
using Core.UseCases.Campaigns.Handlers;
using Core.UseCases.Campaigns.Queries;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Core.Tests.UseCases.Campaigns;

public sealed class GetCampaignOptionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsRegistrationConditionOptionsFromTheBackendContract()
    {
        var handler = new GetCampaignOptionsQueryHandler();

        var result = await handler.Handle(new GetCampaignOptionsQuery(), CancellationToken.None);

        var conditionOptions = result.EventTypes.Should().ContainSingle().Which.Condition.Options;
        conditionOptions.Should().BeEquivalentTo(
            [
                new
                {
                    Code = CustomerRegistrationConditionOptionCodes.AllRegistrations,
                    Sources = Array.Empty<string>(),
                    Supported = true
                },
                new
                {
                    Code = CustomerRegistrationConditionOptionCodes.NormalRegistration,
                    Sources = new[] { CustomerRegistrationSources.Normal },
                    Supported = true
                },
                new
                {
                    Code = CustomerRegistrationConditionOptionCodes.ReferralRegistration,
                    Sources = new[] { CustomerRegistrationSources.Referral },
                    Supported = true
                }
            ]);
    }
}
