using Core.Entities.Events;

namespace Core.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(OutgoingEvent message, CancellationToken cancellationToken);
}
