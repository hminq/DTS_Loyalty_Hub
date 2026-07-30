namespace Messaging.Contracts.Outbox;

public static class OutboxMessageStatuses
{
    public const string Pending = "PENDING";

    public const string Published = "PUBLISHED";

    public const string Failed = "FAILED";
}
