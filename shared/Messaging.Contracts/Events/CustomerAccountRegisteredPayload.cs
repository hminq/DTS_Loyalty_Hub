namespace Messaging.Contracts.Events;

public sealed record CustomerAccountRegisteredPayload(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string FullName);
