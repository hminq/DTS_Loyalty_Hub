using System.Text.Json;
using Campaign.Contracts.Actions;
using Campaign.Contracts.Conditions;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using FluentAssertions;

namespace Core.Tests.Contracts;

public sealed class CampaignLevel2ContractTests
{
    private readonly CampaignDefinitionCatalog _catalog = CampaignDefinitionCatalog.BuiltIn;
    private readonly CampaignConditionParser _conditionParser = new();
    private readonly CampaignConditionEvaluator _conditionEvaluator = new();
    private readonly CampaignConditionCompatibilityAnalyzer _compatibilityAnalyzer = new();
    private readonly CampaignActionBindingParser _actionBindingParser = new();

    [Fact]
    public void BuiltInCatalog_ShouldExposeRegistrationEventTargetsAndIssuePointAction()
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");
        var actionDefinition = _catalog.GetAction(ActionTypes.IssuePoint);

        eventDefinition.PrimaryTargetSelector.Should().Be(CustomerRegistrationTargetSelectors.EventCustomer);
        var sourceField = eventDefinition.ConditionFields.Should().ContainSingle(field =>
            field.Code == "source" &&
            field.DataType == CampaignConditionFieldTypes.Enum).Subject;
        sourceField.Options.Should().Equal("NORMAL", "REFERRAL");
        eventDefinition.Targets.Select(target => target.Selector).Should().Equal(
            CustomerRegistrationTargetSelectors.EventCustomer,
            CustomerRegistrationTargetSelectors.Referrer);

        actionDefinition.RequiredTargetKind.Should().Be(CampaignTargetKinds.Customer);
        actionDefinition.Parameters.Should().ContainSingle(parameter =>
            parameter.Code == "amount" &&
            parameter.DataType == "DECIMAL" &&
            parameter.Required &&
            parameter.MinimumExclusive == 0 &&
            parameter.Scale == 2);
    }

    [Fact]
    public void CatalogConstructor_DuplicateActionCodes_ShouldFailFast()
    {
        var action = _catalog.GetAction(ActionTypes.IssuePoint);

        var act = () => new CampaignDefinitionCatalog(
            _catalog.Events,
            [action, action],
            _catalog.Operators);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CAMPAIGN_ACTION_CODE_DUPLICATE*");
    }

    [Fact]
    public void ConditionParser_ShouldCanonicalizeEnumPredicates()
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");

        var result = _conditionParser.Parse(
            """
            {
              "all": [
                { "operator": "in", "value": ["referral", "normal"], "field": "SOURCE" }
              ]
            }
            """,
            eventDefinition);

        result.IsValid.Should().BeTrue();
        result.CanonicalJson.Should().Be(
            """{"all":[{"field":"source","operator":"IN","value":["NORMAL","REFERRAL"]}]}""");
    }

    [Fact]
    public void ConditionEvaluator_ShouldMatchConfiguredFacts()
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");
        var result = _conditionParser.Parse(
            """{"all":[{"field":"source","operator":"EQUALS","value":"REFERRAL"}]}""",
            eventDefinition);

        _conditionEvaluator.Matches(
                result.Condition!,
                eventDefinition,
                new Dictionary<string, string> { ["source"] = "REFERRAL" })
            .Should()
            .BeTrue();
        _conditionEvaluator.Matches(
                result.Condition!,
                eventDefinition,
                new Dictionary<string, string> { ["source"] = "NORMAL" })
            .Should()
            .BeFalse();
    }

    [Fact]
    public void ConditionParser_DuplicatePredicates_ShouldReturnInvalid()
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");

        var result = _conditionParser.Parse(
            """
            {
              "all": [
                { "field": "source", "operator": "EQUALS", "value": "NORMAL" },
                { "field": "SOURCE", "operator": "equals", "value": "normal" }
              ]
            }
            """,
            eventDefinition);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Code == "CAMPAIGN_CONDITION_DUPLICATE_PREDICATE");
    }

    [Fact]
    public void CompatibilityAnalyzer_ShouldDetectTargetApplicabilityOverlap()
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");
        var referrerTarget = eventDefinition.Targets.Single(target =>
            target.Selector == CustomerRegistrationTargetSelectors.Referrer);

        var normalCondition = _conditionParser.Parse(
            """{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}]}""",
            eventDefinition).Condition!;

        _compatibilityAnalyzer.CanOverlap(
                CampaignCondition.MatchAll,
                referrerTarget.Applicability,
                eventDefinition)
            .Should()
            .BeTrue();
        _compatibilityAnalyzer.CanOverlap(
                normalCondition,
                referrerTarget.Applicability,
                eventDefinition)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void ActionBindingParser_ShouldCanonicalizeIssuePointBinding()
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");
        var actionDefinition = _catalog.GetAction(ActionTypes.IssuePoint);

        var result = _actionBindingParser.Parse(
            ActionTypes.IssuePoint,
            """
            {
              "parameters": { "amount": 100 },
              "target": { "selector": "referrer" }
            }
            """,
            eventDefinition,
            actionDefinition);

        result.IsValid.Should().BeTrue();
        result.Parameters.Should().Be(new IssuePointParameters(100m));
        result.CanonicalJson.Should().Be(
            """{"target":{"selector":"REFERRER"},"parameters":{"amount":100}}""");
    }

    [Theory]
    [InlineData("""{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":0}}""")]
    [InlineData("""{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":1.001}}""")]
    [InlineData("""{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50},"calculationType":"PERCENT"}""")]
    [InlineData("""{"target":{"selector":"UNKNOWN"},"parameters":{"amount":50}}""")]
    public void ActionBindingParser_InvalidConfig_ShouldReturnInvalid(string configJson)
    {
        var eventDefinition = _catalog.GetEvent("CUSTOMER_ACCOUNT_REGISTERED");
        var actionDefinition = _catalog.GetAction(ActionTypes.IssuePoint);

        var result = _actionBindingParser.Parse(
            ActionTypes.IssuePoint,
            configJson,
            eventDefinition,
            actionDefinition);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ActionBindingConfig_ShouldDeserializeCanonicalJson()
    {
        var json = """{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}""";

        var config = JsonSerializer.Deserialize<CampaignActionBindingConfig>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        config!.Target.Selector.Should().Be(CustomerRegistrationTargetSelectors.EventCustomer);
        config.Parameters.GetProperty("amount").GetDecimal().Should().Be(50m);
    }
}
