namespace Consumer.Core.Entities.Constants;

public static class CampaignProcessingErrorCodes
{
    public const string EventIdCollision = "EVENT_ID_COLLISION";
    public const string CampaignConfigurationInvalid = "CAMPAIGN_CONFIGURATION_INVALID";
    public const string CampaignActionConfigurationInvalid =
        "CAMPAIGN_ACTION_CONFIGURATION_INVALID";
    public const string EventCustomerNotFound = "EVENT_CUSTOMER_NOT_FOUND";
    public const string ReferrerCustomerNotFound = "REFERRER_CUSTOMER_NOT_FOUND";
    public const string CampaignTargetCustomerNotFound =
        "CAMPAIGN_TARGET_CUSTOMER_NOT_FOUND";
    public const string PointRewardAmountInvalid = "POINT_REWARD_AMOUNT_INVALID";
    public const string CampaignProcessingPersistenceFailed =
        "CAMPAIGN_PROCESSING_PERSISTENCE_FAILED";
    public const string CampaignProcessingUnexpectedError =
        "CAMPAIGN_PROCESSING_UNEXPECTED_ERROR";

    public const string EventBodyInvalid = "EVENT_BODY_INVALID";
    public const string EventIdRequired = "EVENT_ID_REQUIRED";
    public const string EventIdMismatch = "EVENT_ID_MISMATCH";
    public const string EventTypeMismatch = "EVENT_TYPE_MISMATCH";
    public const string EventTypeUnsupported = "EVENT_TYPE_UNSUPPORTED";
    public const string EventRoutingKeyMismatch = "EVENT_ROUTING_KEY_MISMATCH";
    public const string EventOccurredAtInvalid = "EVENT_OCCURRED_AT_INVALID";
    public const string EventPayloadInvalid = "EVENT_PAYLOAD_INVALID";
    public const string EventSourceInvalid = "EVENT_SOURCE_INVALID";
    public const string EventReferrerInvalid = "EVENT_REFERRER_INVALID";
}
