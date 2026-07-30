using Core.UseCases.Events.Models;

namespace Core.Abstractions;

public interface IOutboxWriter
{
    void Add<TPayload>(VersionedOutboxEvent<TPayload> outboxEvent);
}
