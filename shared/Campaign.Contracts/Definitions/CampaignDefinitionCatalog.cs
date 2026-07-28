using Campaign.Contracts.Conditions;
using Campaign.Contracts.Constants;
using Messaging.Contracts.Events;

namespace Campaign.Contracts.Definitions;

public sealed class CampaignDefinitionCatalog
{
    private readonly IReadOnlyDictionary<string, CampaignEventDefinition> _events;
    private readonly IReadOnlyDictionary<string, CampaignActionDefinition> _actions;
    private readonly IReadOnlyDictionary<string, CampaignConditionOperatorDefinition> _operators;

    public CampaignDefinitionCatalog(
        IReadOnlyList<CampaignEventDefinition> events,
        IReadOnlyList<CampaignActionDefinition> actions,
        IReadOnlyList<CampaignConditionOperatorDefinition> operators)
    {
        Events = events.ToArray();
        Actions = actions.ToArray();
        Operators = operators.ToArray();

        var errors = Validate(Events, Actions, Operators);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Campaign definition catalog is invalid: " +
                string.Join("; ", errors.Select(error => $"{error.Code}:{error.Message}")));
        }

        _events = Events.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);
        _actions = Actions.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);
        _operators = Operators.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<CampaignEventDefinition> Events { get; }

    public IReadOnlyList<CampaignActionDefinition> Actions { get; }

    public IReadOnlyList<CampaignConditionOperatorDefinition> Operators { get; }

    public static CampaignDefinitionCatalog BuiltIn { get; } = CreateBuiltIn();

    public CampaignEventDefinition GetEvent(string code) => _events[code];

    public CampaignActionDefinition GetAction(string code) => _actions[code];

    public bool TryGetEvent(string code, out CampaignEventDefinition eventDefinition) =>
        _events.TryGetValue(code, out eventDefinition!);

    public bool TryGetAction(string code, out CampaignActionDefinition actionDefinition) =>
        _actions.TryGetValue(code, out actionDefinition!);

    public bool TryGetOperator(string code, out CampaignConditionOperatorDefinition operatorDefinition) =>
        _operators.TryGetValue(code, out operatorDefinition!);

    private static CampaignDefinitionCatalog CreateBuiltIn()
    {
        var sourceField = new CampaignConditionFieldDefinition(
            "source",
            CampaignConditionFieldTypes.Enum,
            [CampaignConditionOperators.Equals, CampaignConditionOperators.In],
            [CustomerRegistrationSources.Normal, CustomerRegistrationSources.Referral],
            Required: false);

        var registrationEvent = new CampaignEventDefinition(
            EventTypeCodes.CustomerAccountRegistered,
            CustomerRegistrationTargetSelectors.EventCustomer,
            [sourceField],
            [
                new CampaignTargetDefinition(
                    CustomerRegistrationTargetSelectors.EventCustomer,
                    CampaignTargetKinds.Customer,
                    CampaignCondition.MatchAll),
                new CampaignTargetDefinition(
                    CustomerRegistrationTargetSelectors.Referrer,
                    CampaignTargetKinds.Customer,
                    new CampaignCondition(
                    [
                        new CampaignConditionPredicate(
                            sourceField.Code,
                            CampaignConditionOperators.Equals,
                            [CustomerRegistrationSources.Referral])
                    ]))
            ]);

        var issuePointAction = new CampaignActionDefinition(
            ActionTypes.IssuePoint,
            CampaignTargetKinds.Customer,
            [
                new CampaignParameterFieldDefinition(
                    "amount",
                    "DECIMAL",
                    Required: true,
                    MinimumExclusive: 0,
                    Maximum: 9999999999999999.99m,
                    Scale: 2)
            ]);

        return new CampaignDefinitionCatalog(
            [registrationEvent],
            [issuePointAction],
            [
                new CampaignConditionOperatorDefinition(
                    CampaignConditionOperators.Equals,
                    [CampaignConditionFieldTypes.Enum]),
                new CampaignConditionOperatorDefinition(
                    CampaignConditionOperators.In,
                    [CampaignConditionFieldTypes.Enum])
            ]);
    }

    private static IReadOnlyList<CampaignDefinitionError> Validate(
        IReadOnlyList<CampaignEventDefinition> events,
        IReadOnlyList<CampaignActionDefinition> actions,
        IReadOnlyList<CampaignConditionOperatorDefinition> operators)
    {
        var errors = new List<CampaignDefinitionError>();

        AddBlankOrDuplicateErrors(events.Select(item => item.Code), "CAMPAIGN_EVENT", errors);
        AddBlankOrDuplicateErrors(actions.Select(item => item.Code), "CAMPAIGN_ACTION", errors);
        AddBlankOrDuplicateErrors(operators.Select(item => item.Code), "CAMPAIGN_CONDITION_OPERATOR", errors);

        var operatorLookup = operators.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);
        foreach (var eventDefinition in events)
        {
            AddBlankOrDuplicateErrors(
                eventDefinition.ConditionFields.Select(item => item.Code),
                $"CAMPAIGN_EVENT_{eventDefinition.Code}_FIELD",
                errors);
            AddBlankOrDuplicateErrors(
                eventDefinition.Targets.Select(item => item.Selector),
                $"CAMPAIGN_EVENT_{eventDefinition.Code}_TARGET",
                errors);

            if (!eventDefinition.Targets.Any(target =>
                    string.Equals(
                        target.Selector,
                        eventDefinition.PrimaryTargetSelector,
                        StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new CampaignDefinitionError(
                    "CAMPAIGN_EVENT_PRIMARY_TARGET_MISSING",
                    $"Event '{eventDefinition.Code}' primary target is not registered."));
            }

            foreach (var field in eventDefinition.ConditionFields)
            {
                if (field.DataType != CampaignConditionFieldTypes.Enum)
                {
                    errors.Add(new CampaignDefinitionError(
                        "CAMPAIGN_CONDITION_FIELD_TYPE_INVALID",
                        $"Field '{field.Code}' uses unsupported type '{field.DataType}'."));
                }

                foreach (var operatorCode in field.Operators)
                {
                    if (!operatorLookup.TryGetValue(operatorCode, out var operatorDefinition) ||
                        !operatorDefinition.SupportedFieldTypes.Contains(field.DataType, StringComparer.OrdinalIgnoreCase))
                    {
                        errors.Add(new CampaignDefinitionError(
                            "CAMPAIGN_CONDITION_OPERATOR_INVALID",
                            $"Field '{field.Code}' uses unsupported operator '{operatorCode}'."));
                    }
                }
            }

            var conditionParser = new CampaignConditionParser();
            foreach (var target in eventDefinition.Targets)
            {
                var canonicalApplicability = CampaignConditionParser.ToCanonicalJson(target.Applicability);
                var applicabilityResult = conditionParser.Parse(canonicalApplicability, eventDefinition);
                if (!applicabilityResult.IsValid)
                {
                    errors.Add(new CampaignDefinitionError(
                        "CAMPAIGN_TARGET_APPLICABILITY_INVALID",
                        $"Target '{target.Selector}' has invalid applicability."));
                }
            }
        }

        foreach (var action in actions)
        {
            AddBlankOrDuplicateErrors(
                action.Parameters.Select(item => item.Code),
                $"CAMPAIGN_ACTION_{action.Code}_PARAMETER",
                errors);

            foreach (var parameter in action.Parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter.DataType))
                {
                    errors.Add(new CampaignDefinitionError(
                        "CAMPAIGN_ACTION_PARAMETER_TYPE_INVALID",
                        $"Action '{action.Code}' parameter '{parameter.Code}' has no data type."));
                }
            }
        }

        return errors;
    }

    private static void AddBlankOrDuplicateErrors(
        IEnumerable<string> codes,
        string subject,
        List<CampaignDefinitionError> errors)
    {
        var normalizedCodes = codes.Select(code => code?.Trim() ?? string.Empty).ToArray();
        if (normalizedCodes.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add(new CampaignDefinitionError(
                $"{subject}_CODE_BLANK",
                $"{subject} contains a blank code."));
        }

        var duplicate = normalizedCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .GroupBy(code => code, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            errors.Add(new CampaignDefinitionError(
                $"{subject}_CODE_DUPLICATE",
                $"{subject} contains duplicate code '{duplicate.Key}'."));
        }
    }
}
