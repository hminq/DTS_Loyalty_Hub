using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Messaging.Contracts.Events;

namespace Core.Services;

public sealed class CustomerAccountRegisteredEventValidator
    : ICustomerAccountRegisteredEventValidator
{
    private static readonly HashSet<string> EnvelopeProperties =
    [
        "eventId",
        "eventType",
        "routingKey",
        "occurredAt",
        "data"
    ];

    private static readonly HashSet<string> DataProperties =
    [
        "userId",
        "customerId",
        "source",
        "referrerCustomerId"
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public ValidatedCustomerAccountRegisteredEvent Validate(
        ReadOnlyMemory<byte> body,
        string? messageId,
        string? messageType,
        string? deliveryRoutingKey)
    {
        EnsureJsonShape(body);

        OutgoingEvent<CustomerAccountRegisteredData> envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<OutgoingEvent<CustomerAccountRegisteredData>>(
                body.Span,
                SerializerOptions)
                ?? throw ValidationError(CampaignProcessingErrorCodes.EventBodyInvalid);
        }
        catch (CampaignEventValidationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        ValidateEnvelope(envelope, messageId, messageType, deliveryRoutingKey);

        var normalizedOccurredAt = TruncateToPostgreSqlMicroseconds(envelope.OccurredAt);
        var normalizedEnvelope = new OutgoingEvent<CustomerAccountRegisteredData>(
            envelope.EventId,
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            normalizedOccurredAt,
            new CustomerAccountRegisteredData(
                envelope.Data.UserId,
                envelope.Data.CustomerId,
                envelope.Data.Source,
                envelope.Data.ReferrerCustomerId));
        var normalizedPayload = JsonSerializer.Serialize(normalizedEnvelope, SerializerOptions);
        var payloadHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPayload)))
            .ToLowerInvariant();

        return new ValidatedCustomerAccountRegisteredEvent(
            normalizedEnvelope.EventId,
            normalizedEnvelope.EventType,
            normalizedEnvelope.RoutingKey,
            normalizedEnvelope.OccurredAt,
            normalizedEnvelope.Data.UserId,
            normalizedEnvelope.Data.CustomerId,
            normalizedEnvelope.Data.Source,
            normalizedEnvelope.Data.ReferrerCustomerId,
            normalizedPayload,
            payloadHash);
    }

    private static void EnsureJsonShape(ReadOnlyMemory<byte> body)
    {
        if (body.IsEmpty)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventBodyInvalid);
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            EnsureExactObject(
                root,
                EnvelopeProperties,
                CampaignProcessingErrorCodes.EventBodyInvalid);

            if (!root.TryGetProperty("data", out var data))
            {
                throw ValidationError(CampaignProcessingErrorCodes.EventPayloadInvalid);
            }

            EnsureExactObject(
                data,
                DataProperties,
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }
        catch (CampaignEventValidationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventBodyInvalid);
        }
    }

    private static void EnsureExactObject(
        JsonElement element,
        IReadOnlySet<string> allowedProperties,
        string errorCode)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw ValidationError(errorCode);
        }

        var observedProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!allowedProperties.Contains(property.Name) ||
                !observedProperties.Add(property.Name))
            {
                throw ValidationError(errorCode);
            }
        }
    }

    private static void ValidateEnvelope(
        OutgoingEvent<CustomerAccountRegisteredData> envelope,
        string? messageId,
        string? messageType,
        string? deliveryRoutingKey)
    {
        if (envelope.EventId == Guid.Empty)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventIdRequired);
        }

        if (!Guid.TryParse(messageId, out var metadataEventId) ||
            metadataEventId != envelope.EventId)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventIdMismatch);
        }

        if (!string.Equals(messageType, envelope.EventType, StringComparison.Ordinal))
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventTypeMismatch);
        }

        if (!string.Equals(
                envelope.EventType,
                EventTypeCodes.CustomerAccountRegistered,
                StringComparison.Ordinal))
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventTypeUnsupported);
        }

        if (!string.Equals(
                envelope.RoutingKey,
                EventRoutingKeys.CustomerAccountRegistered,
                StringComparison.Ordinal) ||
            !string.Equals(
                deliveryRoutingKey,
                envelope.RoutingKey,
                StringComparison.Ordinal))
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventRoutingKeyMismatch);
        }

        if (envelope.OccurredAt == default ||
            envelope.OccurredAt.Kind != DateTimeKind.Utc)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventOccurredAtInvalid);
        }

        if (envelope.Data is null ||
            envelope.Data.UserId == Guid.Empty ||
            envelope.Data.CustomerId == Guid.Empty)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        if (envelope.Data.Source != CustomerRegistrationSources.Normal &&
            envelope.Data.Source != CustomerRegistrationSources.Referral)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventSourceInvalid);
        }

        if (envelope.Data.Source == CustomerRegistrationSources.Normal &&
            envelope.Data.ReferrerCustomerId is not null)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventReferrerInvalid);
        }

        if (envelope.Data.Source == CustomerRegistrationSources.Referral &&
            (!envelope.Data.ReferrerCustomerId.HasValue ||
             envelope.Data.ReferrerCustomerId.Value == Guid.Empty))
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventReferrerInvalid);
        }

        if (envelope.Data.ReferrerCustomerId == envelope.Data.CustomerId)
        {
            throw ValidationError(CampaignProcessingErrorCodes.EventReferrerInvalid);
        }
    }

    private static CampaignEventValidationException ValidationError(string errorCode)
    {
        return new CampaignEventValidationException(errorCode);
    }

    private static DateTime TruncateToPostgreSqlMicroseconds(DateTime value)
    {
        const long ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
        return new DateTime(
            value.Ticks - value.Ticks % ticksPerMicrosecond,
            DateTimeKind.Utc);
    }
}
