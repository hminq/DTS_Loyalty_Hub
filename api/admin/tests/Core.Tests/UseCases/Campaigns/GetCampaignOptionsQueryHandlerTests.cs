using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.Campaigns;
using Core.UseCases.Campaigns.Handlers;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;
using System.Text.Json;

namespace Core.Tests.UseCases.Campaigns;

public sealed class GetCampaignOptionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsRegistrationConditionOptionsFromTheBackendContract()
    {
        var repository = new Mock<ICampaignEventDefinitionRepository>();
        repository.Setup(item => item.GetSelectableVersionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([RegistrationDefinition()]);
        var handler = new GetCampaignOptionsQueryHandler(
            new CampaignConfigurationService(),
            repository.Object);

        var result = await handler.Handle(new GetCampaignOptionsQuery(), CancellationToken.None);

        var eventType = result.EventTypeVersions.Should().ContainSingle().Subject;
        eventType.Condition.Combinators.Should().Equal("ALL");
        eventType.Condition.Fields.Should().ContainSingle(field =>
            field.Code == "username" &&
            field.DataType == CampaignConditionFieldTypes.String);
        eventType.Targets.Select(target => target.Selector).Should().Equal("REGISTERED_CUSTOMER");

        var actionType = result.ActionTypes.Should().ContainSingle().Subject;
        actionType.Code.Should().Be(ActionTypes.IssuePoint);
        actionType.RequiredTargetKind.Should().Be(CampaignTargetKinds.Customer);
        actionType.Parameters.Should().ContainSingle(parameter =>
            parameter.Code == "amount" &&
            parameter.DataType == "DECIMAL");
    }

    private static CampaignEventDefinitionResult RegistrationDefinition()
    {
        var schema = new EventPayloadSchema(
            [
                new EventPayloadFieldSchema("username", EventPayloadDataTypes.String, null, true, true),
                new EventPayloadFieldSchema("registeredCustomerId", EventPayloadDataTypes.String, "UUID", true, true)
            ],
            [
                new EventTargetSchema("REGISTERED_CUSTOMER", CampaignTargetKinds.Customer, "registeredCustomerId")
            ]);

        return new CampaignEventDefinitionResult(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            EventTypeCodes.CustomerAccountRegistered,
            "customer.account.registered",
            "Customer account registered",
            EventDefinitionStatuses.Active,
            1,
            EventDefinitionVersionStatuses.Published,
            JsonSerializer.Serialize(schema, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
