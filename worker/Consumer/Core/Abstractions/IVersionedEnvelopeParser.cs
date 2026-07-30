using Consumer.Core.Entities.Envelope;

namespace Consumer.Core.Abstractions;

public interface IVersionedEnvelopeParser
{
    RawEventEnvelope Parse(
        ReadOnlySpan<byte> bodyJson,
        string? amqpMessageId,
        string? amqpType);
}
