using Campaign.Contracts.Constants;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class CustomerAccountRegisteredEventRuntimeDefinitionTests
{
    private readonly CustomerAccountRegisteredEventRuntimeDefinition _runtime =
        new(new CustomerAccountRegisteredEventValidator());

    [Fact]
    public void ResolveTarget_EventCustomer_ReturnsPrimaryCustomer()
    {
        var campaignEvent = CreateEvent(CustomerRegistrationSources.Normal, null);

        var result = _runtime.ResolveTarget(
            campaignEvent,
            CustomerRegistrationTargetSelectors.EventCustomer);

        result.Should().Be(new CampaignTargetResolution(
            CampaignTargetResolutionStatuses.Resolved,
            CampaignTargetKinds.Customer,
            campaignEvent.CustomerId,
            null));
    }

    [Fact]
    public void ResolveTarget_NormalReferrer_ReturnsNotApplicable()
    {
        var result = _runtime.ResolveTarget(
            CreateEvent(CustomerRegistrationSources.Normal, null),
            CustomerRegistrationTargetSelectors.Referrer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.NotApplicable);
        result.TargetId.Should().BeNull();
    }

    [Fact]
    public void ResolveTarget_ReferralReferrer_ReturnsResolvedCustomer()
    {
        var referrerCustomerId = Guid.NewGuid();

        var result = _runtime.ResolveTarget(
            CreateEvent(
                CustomerRegistrationSources.Referral,
                referrerCustomerId),
            CustomerRegistrationTargetSelectors.Referrer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Resolved);
        result.TargetKind.Should().Be(CampaignTargetKinds.Customer);
        result.TargetId.Should().Be(referrerCustomerId);
    }

    [Fact]
    public void ResolveTarget_UnknownSelector_ReturnsUnsupported()
    {
        var result = _runtime.ResolveTarget(
            CreateEvent(CustomerRegistrationSources.Normal, null),
            "UNKNOWN");

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Unsupported);
        result.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    [Fact]
    public void GetFact_Source_ReturnsValidatedRegistrationSource()
    {
        var campaignEvent = CreateEvent(
            CustomerRegistrationSources.Referral,
            Guid.NewGuid());

        var fact = _runtime.GetFact(campaignEvent, "source");

        fact.Should().Be(new CampaignFactValue(
            "source",
            CustomerRegistrationSources.Referral));
    }

    private static ValidatedCustomerAccountRegisteredEvent CreateEvent(
        string source,
        Guid? referrerCustomerId)
    {
        return new ValidatedCustomerAccountRegisteredEvent(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            source,
            referrerCustomerId,
            "{}",
            new string('a', 64));
    }
}
