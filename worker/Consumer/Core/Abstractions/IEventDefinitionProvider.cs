using Consumer.Core.Entities.Definitions;

namespace Consumer.Core.Abstractions;

public interface IEventDefinitionProvider
{
    Task<PublishedEventDefinition?> GetDefinitionAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken = default);
}
