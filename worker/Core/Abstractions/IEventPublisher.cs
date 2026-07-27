using Core.Entities;

namespace Core.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken);
}
