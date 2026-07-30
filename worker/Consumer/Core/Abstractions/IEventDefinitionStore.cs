using Consumer.Core.Entities.Definitions;

namespace Consumer.Core.Abstractions;

public interface IEventDefinitionStore
{
    Task<PublishedEventDefinition?> FetchDefinitionAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken = default);
}
