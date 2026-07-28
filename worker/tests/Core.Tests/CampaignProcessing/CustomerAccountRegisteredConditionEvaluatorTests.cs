using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Core.Tests.CampaignProcessing;

public sealed class CustomerAccountRegisteredConditionEvaluatorTests
{
    private readonly CustomerAccountRegisteredConditionEvaluator _evaluator = new();

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"sources\":null}")]
    [InlineData("{\"sources\":[]}")]
    public void IsMatch_UnrestrictedCondition_MatchesEveryValidSource(string conditionJson)
    {
        _evaluator.IsMatch(conditionJson, CreateEvent(CustomerRegistrationSources.Normal))
            .Should().BeTrue();
        _evaluator.IsMatch(conditionJson, CreateEvent(CustomerRegistrationSources.Referral))
            .Should().BeTrue();
    }

    [Fact]
    public void IsMatch_SourceSpecificCondition_MatchesExactSource()
    {
        const string condition = "{\"sources\":[\"NORMAL\"]}";

        _evaluator.IsMatch(condition, CreateEvent(CustomerRegistrationSources.Normal))
            .Should().BeTrue();
        _evaluator.IsMatch(condition, CreateEvent(CustomerRegistrationSources.Referral))
            .Should().BeFalse();
    }

    [Theory]
    [InlineData("{\"sources\":[\"OTHER\"]}")]
    [InlineData("{\"sources\":[\"NORMAL\",\"NORMAL\"]}")]
    [InlineData("{\"sources\":[null]}")]
    [InlineData("{\"unexpected\":true}")]
    [InlineData("null")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-json")]
    public void IsMatch_InvalidCondition_ThrowsConfigurationInvalid(string conditionJson)
    {
        var act = () => _evaluator.IsMatch(
            conditionJson,
            CreateEvent(CustomerRegistrationSources.Normal));

        act.Should()
            .Throw<CampaignConfigurationException>()
            .Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
    }

    private static ValidatedCustomerAccountRegisteredEvent CreateEvent(string source)
    {
        return new ValidatedCustomerAccountRegisteredEvent(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            source,
            source == CustomerRegistrationSources.Referral ? Guid.NewGuid() : null,
            "{}",
            new string('a', 64));
    }
}
