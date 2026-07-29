using Core.UseCases.Campaigns.Results;

namespace Core.Abstractions;

public interface ICampaignConfigurationService
{
    string ParseCondition(
        CampaignEventDefinitionResult eventDefinition,
        string conditionJson);

    (string ActionType, string ActionConfig) ParseAction(
        CampaignEventDefinitionResult eventDefinition,
        string actionType,
        string actionConfigJson);

    void ValidateAction(
        CampaignEventDefinitionResult eventDefinition,
        string actionType,
        string actionConfigJson);

    string GetActionUniquenessKey(
        CampaignEventDefinitionResult eventDefinition,
        string actionType,
        string actionConfigJson);

    CampaignOptionsResult BuildOptions(
        IReadOnlyCollection<CampaignEventDefinitionResult> eventDefinitions);
}
