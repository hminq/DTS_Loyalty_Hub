using Campaign.Contracts.Actions;
using Campaign.Contracts.Conditions;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using Campaign.Contracts.Schedules;
using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Campaigns.Results;

namespace Core.UseCases.Campaigns;

public sealed class CampaignConfigurationService : ICampaignConfigurationService
{
    private readonly CampaignDefinitionCatalog _catalog;
    private readonly CampaignConditionParser _conditionParser;
    private readonly CampaignConditionCompatibilityAnalyzer _compatibilityAnalyzer;
    private readonly CampaignActionBindingParser _actionBindingParser;

    public CampaignConfigurationService()
        : this(
            CampaignDefinitionCatalog.BuiltIn,
            new CampaignConditionParser(),
            new CampaignConditionCompatibilityAnalyzer(),
            new CampaignActionBindingParser())
    {
    }

    public CampaignConfigurationService(
        CampaignDefinitionCatalog catalog,
        CampaignConditionParser conditionParser,
        CampaignConditionCompatibilityAnalyzer compatibilityAnalyzer,
        CampaignActionBindingParser actionBindingParser)
    {
        _catalog = catalog;
        _conditionParser = conditionParser;
        _compatibilityAnalyzer = compatibilityAnalyzer;
        _actionBindingParser = actionBindingParser;
    }

    public (string EventType, string Condition) ParseCondition(
        string eventType,
        string conditionJson)
    {
        if (!_catalog.TryGetEvent(Normalize(eventType), out var eventDefinition))
        {
            throw ValidationError("CAMPAIGN_EVENT_TYPE_INVALID");
        }

        var result = _conditionParser.Parse(conditionJson, eventDefinition);
        if (!result.IsValid || result.CanonicalJson is null)
        {
            throw ValidationError("CAMPAIGN_CONDITION_INVALID");
        }

        return (eventDefinition.Code, result.CanonicalJson);
    }

    public (string ActionType, string ActionConfig) ParseAction(
        string eventType,
        string actionType,
        string actionConfigJson)
    {
        if (!_catalog.TryGetEvent(Normalize(eventType), out var eventDefinition) ||
            !_catalog.TryGetAction(Normalize(actionType), out var actionDefinition))
        {
            throw ValidationError("CAMPAIGN_ACTION_TYPE_INVALID");
        }

        var result = _actionBindingParser.Parse(
            actionDefinition.Code,
            actionConfigJson,
            eventDefinition,
            actionDefinition);
        if (!result.IsValid || result.CanonicalJson is null)
        {
            throw ValidationError("CAMPAIGN_ACTION_CONFIG_INVALID");
        }

        return (actionDefinition.Code, result.CanonicalJson);
    }

    public void EnsureActionCompatibleWithCondition(
        string eventType,
        string conditionJson,
        string actionType,
        string actionConfigJson)
    {
        var (canonicalEventType, canonicalConditionJson) = ParseCondition(eventType, conditionJson);
        var (_, canonicalActionConfig) = ParseAction(canonicalEventType, actionType, actionConfigJson);
        var eventDefinition = _catalog.GetEvent(canonicalEventType);
        var actionDefinition = _catalog.GetAction(Normalize(actionType));

        var condition = _conditionParser.Parse(canonicalConditionJson, eventDefinition).Condition!;
        var bindingResult = _actionBindingParser.Parse(
            actionDefinition.Code,
            canonicalActionConfig,
            eventDefinition,
            actionDefinition);
        if (!bindingResult.IsValid ||
            bindingResult.Binding is null)
        {
            throw ValidationError("CAMPAIGN_ACTION_CONFIG_INVALID");
        }

        var target = eventDefinition.Targets.Single(item =>
            item.Selector == bindingResult.Binding.Target.Selector);
        if (!_compatibilityAnalyzer.CanOverlap(condition, target.Applicability, eventDefinition))
        {
            throw ValidationError("CAMPAIGN_ACTION_CONDITION_INCOMPATIBLE");
        }
    }

    public string GetActionUniquenessKey(
        string eventType,
        string actionType,
        string actionConfigJson)
    {
        var (canonicalActionType, canonicalActionConfig) = ParseAction(
            eventType,
            actionType,
            actionConfigJson);

        return $"{canonicalActionType}:{canonicalActionConfig}";
    }

    public CampaignOptionsResult GetOptions()
    {
        return new CampaignOptionsResult(
            CampaignStatuses.All,
            new CampaignScheduleOptionsResult(
                CampaignScheduleDefaults.TimeZone,
                CampaignScheduleDays.CanonicalOrder),
            _catalog.Events.Select(eventDefinition => new CampaignEventTypeOptionResult(
                    eventDefinition.Code,
                    new CampaignConditionOptionsResult(
                        ["ALL"],
                        eventDefinition.ConditionFields.Select(field =>
                            new CampaignConditionFieldOptionResult(
                                field.Code,
                                field.DataType,
                                field.Operators.ToArray(),
                                field.Options.ToArray()))
                            .ToArray(),
                        BuildConditionPresets(eventDefinition)),
                    eventDefinition.Targets.Select(target =>
                        new CampaignTargetOptionResult(
                            target.Selector,
                            target.TargetKind,
                            CampaignConditionParser.ToCanonicalJson(target.Applicability)))
                        .ToArray()))
                .ToArray(),
            _catalog.Actions.Select(actionDefinition => new CampaignActionTypeOptionResult(
                    actionDefinition.Code,
                    actionDefinition.RequiredTargetKind,
                    actionDefinition.Parameters.Select(parameter =>
                        new CampaignParameterFieldOptionResult(
                            parameter.Code,
                            parameter.DataType,
                            parameter.Required,
                            parameter.MinimumExclusive,
                            parameter.Maximum,
                            parameter.Scale))
                        .ToArray()))
                .ToArray());
    }

    private static IReadOnlyCollection<CampaignConditionPresetOptionResult> BuildConditionPresets(
        CampaignEventDefinition eventDefinition)
    {
        var sourceField = eventDefinition.ConditionFields.FirstOrDefault(field => field.Code == "source");
        if (sourceField is null)
        {
            return [];
        }

        return
        [
            new CampaignConditionPresetOptionResult(
                CustomerRegistrationConditionOptionCodes.AllRegistrations,
                CampaignConditionParser.ToCanonicalJson(CampaignCondition.MatchAll)),
            new CampaignConditionPresetOptionResult(
                CustomerRegistrationConditionOptionCodes.NormalRegistration,
                CampaignConditionParser.ToCanonicalJson(new CampaignCondition(
                [
                    new CampaignConditionPredicate(
                        sourceField.Code,
                        CampaignConditionOperators.Equals,
                        ["NORMAL"])
                ]))),
            new CampaignConditionPresetOptionResult(
                CustomerRegistrationConditionOptionCodes.ReferralRegistration,
                CampaignConditionParser.ToCanonicalJson(new CampaignCondition(
                [
                    new CampaignConditionPredicate(
                        sourceField.Code,
                        CampaignConditionOperators.Equals,
                        ["REFERRAL"])
                ])))
        ];
    }

    private static string Normalize(string? value) =>
        value?.Trim().ToUpperInvariant() ?? string.Empty;

    private static DomainException ValidationError(string code) =>
        new(code, DomainErrorType.Validation);
}
