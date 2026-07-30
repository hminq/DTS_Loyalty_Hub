namespace Consumer.Core.Abstractions;

public interface ICampaignActionExecutorRegistry
{
    ICampaignActionExecutor GetRequired(string actionType);
}
