using Core.Entities.Campaigns;

namespace Core.Abstractions;

public interface ICustomerAccountRegisteredEventValidator
{
    ValidatedCustomerAccountRegisteredEvent Validate(
        ReadOnlyMemory<byte> body,
        string? messageId,
        string? messageType,
        string? deliveryRoutingKey);
}
