using Core.UseCases.Campaigns.Results;

namespace Core.Abstractions;

public interface ICampaignConfigurationService
{
    (string EventType, string Condition) ParseCondition(
        string eventType,
        string conditionJson);

    (string ActionType, string ActionConfig) ParseAction(
        string eventType,
        string actionType,
        string actionConfigJson);

    void EnsureActionCompatibleWithCondition(
        string eventType,
        string conditionJson,
        string actionType,
        string actionConfigJson);

    string GetActionUniquenessKey(
        string eventType,
        string actionType,
        string actionConfigJson);

    CampaignOptionsResult GetOptions();
}
