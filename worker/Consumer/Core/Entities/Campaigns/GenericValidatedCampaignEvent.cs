using Consumer.Core.Entities.Definitions;

namespace Consumer.Core.Entities.Campaigns;

public sealed record GenericValidatedCampaignEvent(
    Guid EventId,
    Guid EventTypeId,
    Guid EventTypeVersionId,
    string EventType,
    int EventVersion,
    string RoutingKey,
    DateTime OccurredAt,
    PublishedEventDefinition Definition,
    IReadOnlyDictionary<string, ValidatedPayloadValue> PayloadValues,
    string NormalizedPayload,
    string PayloadHash)
    : IValidatedCampaignEvent;

public sealed record ValidatedPayloadValue(
    string Code,
    string Type,
    string? Format,
    object Value)
{
    public string AsFactString()
    {
        return Value switch
        {
            decimal decimalValue => decimalValue.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            bool boolValue => boolValue ? "true" : "false",
            _ => Value.ToString() ?? string.Empty
        };
    }
}
