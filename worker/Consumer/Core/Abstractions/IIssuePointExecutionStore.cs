using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface IIssuePointExecutionStore
{
    Task PrepareIssuePointExecutionAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default);

    void ApplyIssuePoint(IssuePointMutation mutation);
}
