using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface ICustomerAccountRegisteredEventValidator
{
    ValidatedCustomerAccountRegisteredEvent Validate(
        ReadOnlyMemory<byte> body,
        string? messageId,
        string? messageType,
        string? deliveryRoutingKey);
}
