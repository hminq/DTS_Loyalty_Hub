using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface IIssuePointExecutionStore
{
    Task LockCustomerPointsAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default);

    void ApplyIssuePoint(IssuePointMutation mutation);
}
