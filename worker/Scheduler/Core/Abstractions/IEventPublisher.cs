using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken);
}
