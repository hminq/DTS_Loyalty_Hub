using Campaign.Contracts.Campaigns.Actions;

namespace Core.Abstractions;

public interface IIssuePointActionConfigParser
{
    IssuePointActionConfig Parse(string actionType, string actionConfigJson);
}
