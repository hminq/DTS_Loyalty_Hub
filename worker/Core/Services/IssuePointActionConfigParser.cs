using System.Text.Json;
using System.Text.Json.Serialization;
using Campaign.Contracts.Campaigns.Actions;
using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Services;

public sealed class IssuePointActionConfigParser : IIssuePointActionConfigParser
{
    private const decimal MaximumNumeric18Scale2 = 9999999999999999.99m;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public IssuePointActionConfig Parse(string actionType, string actionConfigJson)
    {
        if (actionType != ActionTypes.IssuePoint ||
            string.IsNullOrWhiteSpace(actionConfigJson))
        {
            throw InvalidConfiguration();
        }

        IssuePointActionConfig config;
        try
        {
            config = JsonSerializer.Deserialize<IssuePointActionConfig>(
                actionConfigJson,
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

        if (config.CalculationType != PointCalculationTypes.FixedAmount ||
            config.Recipient != PointRecipients.EventCustomer &&
            config.Recipient != PointRecipients.Referrer ||
            config.CalculationBase is not null ||
            config.Percentage is not null ||
            config.MaximumPoints is not null)
        {
            throw InvalidConfiguration();
        }

        if (config.Amount is null or <= 0 ||
            config.Amount > MaximumNumeric18Scale2 ||
            decimal.Round(config.Amount.Value, 2) != config.Amount.Value)
        {
            throw new CampaignConfigurationException(
                CampaignProcessingErrorCodes.PointRewardAmountInvalid);
        }

        return new IssuePointActionConfig(
            PointCalculationTypes.FixedAmount,
            config.Recipient,
            config.Amount,
            null,
            null,
            null);
    }

    private static CampaignConfigurationException InvalidConfiguration()
    {
        return new CampaignConfigurationException(
            CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }
}
