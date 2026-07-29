namespace Scheduler.Core.Entities.Constants;

public static class OutboxDispatchErrorCodes
{
    public const string BrokerUnreachable = "OUTBOX_BROKER_UNREACHABLE";
    public const string PublishNotRouted = "OUTBOX_PUBLISH_NOT_ROUTED";
    public const string PublishNotConfirmed = "OUTBOX_PUBLISH_NOT_CONFIRMED";
    public const string UnexpectedError = "OUTBOX_PUBLISH_UNEXPECTED_ERROR";
}
