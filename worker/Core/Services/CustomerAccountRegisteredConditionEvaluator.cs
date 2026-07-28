using System.Text.Json;
using System.Text.Json.Serialization;
using Campaign.Contracts.Campaigns.Conditions;
using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Messaging.Contracts.Events;

namespace Core.Services;

public sealed class CustomerAccountRegisteredConditionEvaluator
    : ICustomerAccountRegisteredConditionEvaluator
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public bool IsMatch(
        string conditionJson,
        ValidatedCustomerAccountRegisteredEvent campaignEvent)
    {
        if (string.IsNullOrWhiteSpace(conditionJson))
        {
            throw InvalidConfiguration();
        }

        CustomerAccountRegisteredCondition condition;

        try
        {
            condition = JsonSerializer.Deserialize<CustomerAccountRegisteredCondition>(
                conditionJson,
                SerializerOptions)
                ?? throw InvalidConfiguration();
        }
        catch (CampaignConfigurationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw InvalidConfiguration();
        }

        if (condition.Sources is null || condition.Sources.Count == 0)
        {
            return true;
        }

        if (condition.Sources.Any(source =>
                source != CustomerRegistrationSources.Normal &&
                source != CustomerRegistrationSources.Referral) ||
            condition.Sources.Distinct(StringComparer.Ordinal).Count() !=
            condition.Sources.Count)
        {
            throw InvalidConfiguration();
        }

        return condition.Sources.Contains(campaignEvent.Source, StringComparer.Ordinal);
    }

    private static CampaignConfigurationException InvalidConfiguration()
    {
        return new CampaignConfigurationException(
            CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
    }
}
