using Campaign.Contracts.Actions;
using Campaign.Contracts.Conditions;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using Campaign.Contracts.Schedules;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Campaigns.Results;
using Messaging.Contracts.Events;

namespace Core.UseCases.Campaigns;

public sealed class CampaignConfigurationService : ICampaignConfigurationService
{
    private readonly CampaignActionCatalog _actionCatalog;
    private readonly CampaignConditionParser _conditionParser;
    private readonly CampaignActionBindingParser _actionBindingParser;

    public CampaignConfigurationService()
        : this(
            CampaignActionCatalog.BuiltIn,
            new CampaignConditionParser(),
            new CampaignActionBindingParser())
    {
    }

    public CampaignConfigurationService(
        CampaignActionCatalog actionCatalog,
        CampaignConditionParser conditionParser,
        CampaignActionBindingParser actionBindingParser)
    {
        _actionCatalog = actionCatalog;
        _conditionParser = conditionParser;
        _actionBindingParser = actionBindingParser;
    }

    public string ParseCondition(
        CampaignEventDefinitionResult eventDefinition,
        string conditionJson)
    {
        EnsureSelectable(eventDefinition);
        var runtimeDefinition = ToRuntimeDefinition(eventDefinition);
        var result = _conditionParser.Parse(conditionJson, runtimeDefinition);
        if (!result.IsValid || result.CanonicalJson is null)
        {
            throw ValidationError("CAMPAIGN_CONDITION_INVALID");
        }

        return result.CanonicalJson;
    }

    public (string ActionType, string ActionConfig) ParseAction(
        CampaignEventDefinitionResult eventDefinition,
        string actionType,
        string actionConfigJson)
    {
        EnsureSelectable(eventDefinition);
        if (!_actionCatalog.TryGetAction(Normalize(actionType), out var actionDefinition))
        {
            throw ValidationError("CAMPAIGN_ACTION_TYPE_INVALID");
        }

        var result = _actionBindingParser.Parse(
            actionDefinition.Code,
            actionConfigJson,
            ToRuntimeDefinition(eventDefinition),
            actionDefinition);
        if (!result.IsValid || result.CanonicalJson is null)
        {
            throw ValidationError(
                result.Errors.Any(error => error.Code.Contains("TARGET", StringComparison.Ordinal))
                    ? "CAMPAIGN_ACTION_TARGET_INVALID"
                    : "CAMPAIGN_ACTION_CONFIG_INVALID");
        }

        return (actionDefinition.Code, result.CanonicalJson);
    }

    public void ValidateAction(
        CampaignEventDefinitionResult eventDefinition,
        string actionType,
        string actionConfigJson)
    {
        ParseAction(eventDefinition, actionType, actionConfigJson);
    }

    public string GetActionUniquenessKey(
        CampaignEventDefinitionResult eventDefinition,
        string actionType,
        string actionConfigJson)
    {
        var (canonicalActionType, canonicalActionConfig) = ParseAction(
            eventDefinition,
            actionType,
            actionConfigJson);

        return $"{canonicalActionType}:{canonicalActionConfig}";
    }

    public CampaignOptionsResult BuildOptions(
        IReadOnlyCollection<CampaignEventDefinitionResult> eventDefinitions)
    {
        return new CampaignOptionsResult(
            CampaignStatuses.All,
            new CampaignScheduleOptionsResult(
                CampaignScheduleDefaults.TimeZone,
                CampaignScheduleDays.CanonicalOrder),
            eventDefinitions.Select(eventDefinition =>
                {
                    var schema = DeserializeSchema(eventDefinition.PayloadSchema);
                    return new CampaignEventTypeVersionOptionResult(
                        eventDefinition.EventTypeId,
                        eventDefinition.EventTypeVersionId,
                        eventDefinition.Code,
                        eventDefinition.RoutingKey,
                        eventDefinition.Name,
                        eventDefinition.Version,
                        new CampaignConditionOptionsResult(
                            ["ALL"],
                            schema.Fields
                                .Where(field => field.Conditionable)
                                .Select(field => new CampaignConditionFieldOptionResult(
                                    field.Code,
                                    field.Type,
                                    field.Format,
                                    field.Required,
                                    GetOperators(field.Type).ToArray(),
                                    Array.Empty<string>()))
                                .ToArray()),
                        schema.Targets.Select(target =>
                            new CampaignTargetOptionResult(
                                target.Selector,
                                target.Kind,
                                target.IdField))
                            .ToArray());
                })
                .ToArray(),
            _actionCatalog.Actions.Select(actionDefinition => new CampaignActionTypeOptionResult(
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

    private static string Normalize(string? value) =>
        value?.Trim().ToUpperInvariant() ?? string.Empty;

    private static DomainException ValidationError(string code) =>
        new(code, DomainErrorType.Validation);

    private static void EnsureSelectable(CampaignEventDefinitionResult eventDefinition)
    {
        if (eventDefinition.EventTypeStatus != EventDefinitionStatuses.Active ||
            eventDefinition.VersionStatus != EventDefinitionVersionStatuses.Published)
        {
            throw ValidationError("CAMPAIGN_EVENT_TYPE_VERSION_NOT_SELECTABLE");
        }
    }

    private static CampaignEventDefinition ToRuntimeDefinition(
        CampaignEventDefinitionResult eventDefinition)
    {
        var schema = DeserializeSchema(eventDefinition.PayloadSchema);
        return new CampaignEventDefinition(
            eventDefinition.Code,
            schema.Targets.FirstOrDefault()?.Selector ?? string.Empty,
            schema.Fields
                .Where(field => field.Conditionable)
                .Select(field => new CampaignConditionFieldDefinition(
                    field.Code,
                    field.Type,
                    GetOperators(field.Type).ToArray(),
                    Array.Empty<string>(),
                    field.Required,
                    field.Format,
                    field.Conditionable))
                .ToArray(),
            schema.Targets
                .Select(target => new CampaignTargetDefinition(
                    target.Selector,
                    target.Kind,
                    CampaignCondition.MatchAll))
                .ToArray());
    }

    private static EventPayloadSchema DeserializeSchema(string payloadSchema)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<EventPayloadSchema>(
                    payloadSchema,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    })
                ?? throw ValidationError("CAMPAIGN_CONDITION_INVALID");
        }
        catch (System.Text.Json.JsonException)
        {
            throw ValidationError("CAMPAIGN_CONDITION_INVALID");
        }
    }

    private static IReadOnlyCollection<string> GetOperators(string fieldType)
    {
        return fieldType switch
        {
            EventPayloadDataTypes.String =>
            [
                ConditionOperatorCodes.Equal,
                ConditionOperatorCodes.NotEquals,
                ConditionOperatorCodes.Contains
            ],
            EventPayloadDataTypes.Number =>
            [
                ConditionOperatorCodes.Equal,
                ConditionOperatorCodes.NotEquals,
                ConditionOperatorCodes.GreaterThan,
                ConditionOperatorCodes.GreaterThanOrEquals,
                ConditionOperatorCodes.LessThan,
                ConditionOperatorCodes.LessThanOrEquals
            ],
            EventPayloadDataTypes.Boolean =>
            [
                ConditionOperatorCodes.Equal,
                ConditionOperatorCodes.NotEquals
            ],
            _ => Array.Empty<string>()
        };
    }
}
