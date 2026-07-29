using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class CampaignEventRuntimeRegistryTests
{
    [Fact]
    public void Constructor_RuntimeMatchesCatalog_ResolvesRuntime()
    {
        var runtime = new StubRuntime(EventTypeCodes.CustomerAccountRegistered);

        var registry = new CampaignEventRuntimeRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [runtime]);

        registry.GetRequired(EventTypeCodes.CustomerAccountRegistered)
            .Should().BeSameAs(runtime);
    }

    [Fact]
    public void Constructor_MissingCatalogRuntime_FailsStartupIntegrity()
    {
        var act = () => new CampaignEventRuntimeRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            []);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Missing=[CUSTOMER_ACCOUNT_REGISTERED]*");
    }

    [Fact]
    public void Constructor_RuntimeAbsentFromCatalog_FailsStartupIntegrity()
    {
        var act = () => new CampaignEventRuntimeRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [
                new StubRuntime(EventTypeCodes.CustomerAccountRegistered),
                new StubRuntime("UNKNOWN_EVENT")
            ]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown=[UNKNOWN_EVENT]*");
    }

    [Fact]
    public void Constructor_DuplicateRuntime_FailsStartupIntegrity()
    {
        var act = () => new CampaignEventRuntimeRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [
                new StubRuntime(EventTypeCodes.CustomerAccountRegistered),
                new StubRuntime(EventTypeCodes.CustomerAccountRegistered)
            ]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duplicate campaign event runtime registration*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("UNKNOWN_EVENT")]
    [InlineData("customer_account_registered")]
    public void GetRequired_UnknownOrNonCanonicalType_ThrowsUnsupported(string? eventType)
    {
        var registry = new CampaignEventRuntimeRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [new StubRuntime(EventTypeCodes.CustomerAccountRegistered)]);

        var act = () => registry.GetRequired(eventType);

        act.Should()
            .Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.EventTypeUnsupported);
    }

    private sealed class StubRuntime(string eventType)
        : ICampaignEventRuntimeDefinition
    {
        public string EventType { get; } = eventType;

        public IValidatedCampaignEvent ValidateDelivery(CampaignEventDelivery delivery)
        {
            throw new NotSupportedException();
        }

        public CampaignTargetResolution ResolveTarget(
            IValidatedCampaignEvent campaignEvent,
            string selector)
        {
            throw new NotSupportedException();
        }

        public CampaignFactValue GetFact(
            IValidatedCampaignEvent campaignEvent,
            string fieldCode)
        {
            throw new NotSupportedException();
        }
    }
}
