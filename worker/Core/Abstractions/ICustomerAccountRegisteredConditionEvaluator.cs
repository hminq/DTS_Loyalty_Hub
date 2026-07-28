using Core.Entities.Campaigns;

namespace Core.Abstractions;

public interface ICustomerAccountRegisteredConditionEvaluator
{
    bool IsMatch(
        string conditionJson,
        ValidatedCustomerAccountRegisteredEvent campaignEvent);
}
