using System.Globalization;
using System.Text.Json;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Envelope;
using Consumer.Core.Exceptions;

namespace Consumer.Core.Services;

public sealed class VersionedEnvelopeParser : IVersionedEnvelopeParser
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow
    };

    public RawEventEnvelope Parse(
        ReadOnlySpan<byte> bodyJson,
        string? amqpMessageId,
        string? amqpType)
    {
        if (bodyJson.IsEmpty)
        {
            throw new EventEnvelopeParseException(
                CampaignProcessingErrorCodes.EventBodyInvalid,
                "Envelope body is empty.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(bodyJson.ToArray(), DocumentOptions);
        }
        catch (JsonException ex)
        {
            throw new EventEnvelopeParseException(
                CampaignProcessingErrorCodes.EventBodyInvalid,
                $"Envelope JSON is malformed: {ex.Message}");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new EventEnvelopeParseException(
                    CampaignProcessingErrorCodes.EventBodyInvalid,
                    "Envelope root must be a JSON object.");
            }

            Guid? eventId = null;
            string? eventType = null;
            int? eventVersion = null;
            DateTime? occurredAt = null;
            JsonElement? payloadElement = null;

            int propertyCount = 0;

            foreach (var property in root.EnumerateObject())
            {
                propertyCount++;

                switch (property.Name)
                {
                    case "eventId":
                        if (eventId.HasValue)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "Duplicate eventId property.");
                        }

                        if (property.Value.ValueKind != JsonValueKind.String ||
                            !property.Value.TryGetGuid(out var parsedGuid) ||
                            parsedGuid == Guid.Empty)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventIdRequired,
                                "eventId must be a non-empty UUID string.");
                        }

                        eventId = parsedGuid;
                        break;

                    case "eventType":
                        if (eventType is not null)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "Duplicate eventType property.");
                        }

                        if (property.Value.ValueKind != JsonValueKind.String)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "eventType must be a string.");
                        }

                        var typeValue = property.Value.GetString();
                        if (string.IsNullOrWhiteSpace(typeValue))
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "eventType cannot be empty or whitespace.");
                        }

                        eventType = typeValue;
                        break;

                    case "eventVersion":
                        if (eventVersion.HasValue)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "Duplicate eventVersion property.");
                        }

                        if (property.Value.ValueKind != JsonValueKind.Number ||
                            !property.Value.TryGetInt32(out var parsedVersion) ||
                            parsedVersion <= 0)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "eventVersion must be a positive integer.");
                        }

                        eventVersion = parsedVersion;
                        break;

                    case "occurredAt":
                        if (occurredAt.HasValue)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "Duplicate occurredAt property.");
                        }

                        if (property.Value.ValueKind != JsonValueKind.String)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventOccurredAtInvalid,
                                "occurredAt must be a string timestamp.");
                        }

                        var rawOccurredAt = property.Value.GetString();
                        if (!TryParseUtcTimestamp(rawOccurredAt, out var parsedUtcTime))
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventOccurredAtInvalid,
                                "occurredAt must be a valid UTC timestamp.");
                        }

                        occurredAt = parsedUtcTime;
                        break;

                    case "payload":
                        if (payloadElement.HasValue)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventBodyInvalid,
                                "Duplicate payload property.");
                        }

                        if (property.Value.ValueKind != JsonValueKind.Object)
                        {
                            throw new EventEnvelopeParseException(
                                CampaignProcessingErrorCodes.EventPayloadInvalid,
                                "payload must be a JSON object.");
                        }

                        payloadElement = property.Value.Clone();
                        break;

                    default:
                        throw new EventEnvelopeParseException(
                            CampaignProcessingErrorCodes.EventBodyInvalid,
                            $"Unknown envelope property: {property.Name}");
                }
            }

            if (propertyCount != 5 ||
                !eventId.HasValue ||
                eventType is null ||
                !eventVersion.HasValue ||
                !occurredAt.HasValue ||
                !payloadElement.HasValue)
            {
                throw new EventEnvelopeParseException(
                    CampaignProcessingErrorCodes.EventBodyInvalid,
                    "Envelope must contain exactly the 5 required properties.");
            }

            ValidateAmqpMetadata(eventId.Value, eventType, amqpMessageId, amqpType);

            return new RawEventEnvelope(
                eventId.Value,
                eventType,
                eventVersion.Value,
                occurredAt.Value,
                payloadElement.Value);
        }
    }

    private static void ValidateAmqpMetadata(
        Guid eventId,
        string eventType,
        string? amqpMessageId,
        string? amqpType)
    {
        if (string.IsNullOrWhiteSpace(amqpMessageId))
        {
            throw new EventEnvelopeParseException(
                CampaignProcessingErrorCodes.EventIdRequired,
                "AMQP MessageId header is required.");
        }

        if (!Guid.TryParse(amqpMessageId, out var parsedAmqpMessageId) ||
            parsedAmqpMessageId != eventId)
        {
            throw new EventEnvelopeParseException(
                CampaignProcessingErrorCodes.EventIdMismatch,
                "AMQP MessageId header does not match eventId in envelope.");
        }

        if (string.IsNullOrWhiteSpace(amqpType))
        {
            throw new EventEnvelopeParseException(
                CampaignProcessingErrorCodes.EventTypeMismatch,
                "AMQP Type header is required.");
        }

        if (!string.Equals(amqpType, eventType, StringComparison.Ordinal))
        {
            throw new EventEnvelopeParseException(
                CampaignProcessingErrorCodes.EventTypeMismatch,
                "AMQP Type header does not match eventType in envelope.");
        }
    }

    private static bool TryParseUtcTimestamp(string? raw, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (!raw.EndsWith("Z", StringComparison.Ordinal))
        {
            return false;
        }

        if (DateTimeOffset.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedOffset))
        {
            if (parsedOffset.Offset != TimeSpan.Zero)
            {
                return false;
            }

            result = parsedOffset.UtcDateTime;
            return true;
        }

        return false;
    }
}
