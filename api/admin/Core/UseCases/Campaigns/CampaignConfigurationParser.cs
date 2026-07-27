using System.Text.Json;
using System.Text.Json.Serialization;
using Campaign.Contracts.Campaigns.Actions;
using Campaign.Contracts.Campaigns.Conditions;
using Campaign.Contracts.Constants;
using Core.Exceptions;
using Messaging.Contracts.Events;

namespace Core.UseCases.Campaigns;

public static class CampaignConfigurationParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static (string EventType, string Condition) ParseCondition(
        string eventType,
        string conditionJson)
    {
        var normalizedEventType = Normalize(eventType);

        if (normalizedEventType != EventTypeCodes.CustomerAccountRegistered)
        {
            throw ValidationError("CAMPAIGN_EVENT_TYPE_INVALID");
        }

        try
        {
            var condition = JsonSerializer.Deserialize<CustomerAccountRegisteredCondition>(
                conditionJson,
                SerializerOptions)
                ?? throw ValidationError("CAMPAIGN_CONDITION_INVALID");

            var sources = condition.Sources?
                .Select(Normalize)
                .ToArray();

            if (sources is not null &&
                (sources.Distinct(StringComparer.Ordinal).Count() != sources.Length ||
                 sources.Any(source => source != CustomerRegistrationSources.Normal)))
            {
                throw ValidationError("CAMPAIGN_CONDITION_INVALID");
            }

            return (
                normalizedEventType,
                JsonSerializer.Serialize(
                    new CustomerAccountRegisteredCondition(sources),
                    SerializerOptions));
        }
        catch (DomainException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw ValidationError("CAMPAIGN_CONDITION_INVALID");
        }
    }

    public static (string ActionType, string ActionConfig) ParseAction(
        string eventType,
        string actionType,
        string actionConfigJson)
    {
        if (Normalize(eventType) != EventTypeCodes.CustomerAccountRegistered ||
            Normalize(actionType) != ActionTypes.IssuePoint)
        {
            throw ValidationError("CAMPAIGN_ACTION_TYPE_INVALID");
        }

        try
        {
            var config = JsonSerializer.Deserialize<IssuePointActionConfig>(
                actionConfigJson,
                SerializerOptions)
                ?? throw ValidationError("CAMPAIGN_ACTION_CONFIG_INVALID");

            var calculationType = Normalize(config.CalculationType);
            var recipient = Normalize(config.Recipient);

            if (calculationType != PointCalculationTypes.FixedAmount ||
                recipient != PointRecipients.EventCustomer ||
                config.Amount is null or <= 0 ||
                HasMoreThanTwoDecimalPlaces(config.Amount.Value) ||
                config.CalculationBase is not null ||
                config.Percentage is not null ||
                config.MaximumPoints is not null)
            {
                throw ValidationError("CAMPAIGN_ACTION_CONFIG_INVALID");
            }

            var normalizedConfig = new IssuePointActionConfig(
                calculationType,
                recipient,
                config.Amount,
                null,
                null,
                null);

            return (
                ActionTypes.IssuePoint,
                JsonSerializer.Serialize(normalizedConfig, SerializerOptions));
        }
        catch (DomainException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw ValidationError("CAMPAIGN_ACTION_CONFIG_INVALID");
        }
    }

    private static string Normalize(string? value)
    {
        return value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static bool HasMoreThanTwoDecimalPlaces(decimal value)
    {
        return decimal.Round(value, 2) != value;
    }

    private static DomainException ValidationError(string code)
    {
        return new DomainException(code, DomainErrorType.Validation);
    }
}
