namespace Messaging.Contracts.Events;

public sealed record CustomerAccountRegisteredData(
    Guid UserId,
    Guid CustomerId,
    string Source,
    Guid? ReferrerCustomerId);
