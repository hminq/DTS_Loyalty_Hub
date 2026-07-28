using Campaign.Contracts.Constants;
using Core.UseCases.Campaigns;
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
        var handler = new GetCampaignOptionsQueryHandler(new CampaignConfigurationService());

        var result = await handler.Handle(new GetCampaignOptionsQuery(), CancellationToken.None);

        var eventType = result.EventTypes.Should().ContainSingle().Subject;
        eventType.Condition.Combinators.Should().Equal("ALL");
        eventType.Condition.Fields.Should().ContainSingle(field =>
            field.Code == "source" &&
            field.DataType == CampaignConditionFieldTypes.Enum);
        eventType.Condition.Presets.Select(preset => preset.Code).Should().Equal(
            CustomerRegistrationConditionOptionCodes.AllRegistrations,
            CustomerRegistrationConditionOptionCodes.NormalRegistration,
            CustomerRegistrationConditionOptionCodes.ReferralRegistration);
        eventType.Targets.Select(target => target.Code).Should().Equal("EVENT_CUSTOMER", "REFERRER");

        var actionType = result.ActionTypes.Should().ContainSingle().Subject;
        actionType.Code.Should().Be(ActionTypes.IssuePoint);
        actionType.RequiredTargetKind.Should().Be(CampaignTargetKinds.Customer);
        actionType.Parameters.Should().ContainSingle(parameter =>
            parameter.Code == "amount" &&
            parameter.DataType == "DECIMAL");
    }
}
