using Messaging.Contracts.Events;

namespace Core.Abstractions;

public interface IOutboxWriter
{
    void Add<TData>(OutgoingEvent<TData> outgoingEvent);
}
