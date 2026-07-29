using System.Text.Json;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;

namespace Campaign.Contracts.Actions;

public sealed class CampaignActionBindingParser
{
    private static readonly JsonSerializerOptions CanonicalJsonOptions = new(JsonSerializerDefaults.Web);

    public CampaignActionBindingParseResult Parse(
        string actionType,
        string actionConfigJson,
        CampaignEventDefinition eventDefinition,
        CampaignActionDefinition actionDefinition)
    {
        if (!string.Equals(actionType, actionDefinition.Code, StringComparison.OrdinalIgnoreCase))
        {
            return Invalid("CAMPAIGN_ACTION_TYPE_INVALID", "Campaign action type is not registered.");
        }

        if (string.IsNullOrWhiteSpace(actionConfigJson))
        {
            return Invalid("CAMPAIGN_ACTION_CONFIG_REQUIRED", "Campaign action config is required.");
        }

        try
        {
            using var document = JsonDocument.Parse(actionConfigJson);
            return Parse(document.RootElement, eventDefinition, actionDefinition);
        }
        catch (JsonException)
        {
            return Invalid("CAMPAIGN_ACTION_CONFIG_INVALID_JSON", "Campaign action config must be valid JSON.");
        }
    }

    public CampaignActionBindingParseResult Parse(
        JsonElement actionConfig,
        CampaignEventDefinition eventDefinition,
        CampaignActionDefinition actionDefinition)
    {
        var errors = new List<CampaignDefinitionError>();

        if (actionConfig.ValueKind != JsonValueKind.Object)
        {
            return Invalid("CAMPAIGN_ACTION_CONFIG_INVALID", "Campaign action config must be a JSON object.");
        }

        var rootProperties = actionConfig.EnumerateObject()
            .Select(property => property.Name)
            .ToArray();
        if (rootProperties.Any(name => name is not ("target" or "parameters")) ||
            !rootProperties.Contains("target") ||
            !rootProperties.Contains("parameters"))
        {
            return Invalid(
                "CAMPAIGN_ACTION_CONFIG_INVALID",
                "Campaign action config must contain only target and parameters.");
        }

        var targetElement = actionConfig.GetProperty("target");
        if (targetElement.ValueKind != JsonValueKind.Object)
        {
            return Invalid("CAMPAIGN_ACTION_TARGET_INVALID", "Campaign action target must be an object.");
        }

        var targetProperties = targetElement.EnumerateObject()
            .Select(property => property.Name)
            .ToArray();
        if (targetProperties.Length != 1 ||
            !targetProperties.Contains("selector") ||
            targetElement.GetProperty("selector").ValueKind != JsonValueKind.String)
        {
            return Invalid(
                "CAMPAIGN_ACTION_TARGET_INVALID",
                "Campaign action target must contain one selector string.");
        }

        var selector = targetElement.GetProperty("selector").GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(selector))
        {
            return Invalid("CAMPAIGN_ACTION_TARGET_INVALID", "Campaign action target selector is required.");
        }

        var target = eventDefinition.Targets.FirstOrDefault(candidate =>
            string.Equals(candidate.Selector, selector, StringComparison.Ordinal));
        if (target is null)
        {
            return Invalid("CAMPAIGN_ACTION_TARGET_INVALID", "Campaign action target selector is not registered.");
        }

        if (!string.Equals(target.TargetKind, actionDefinition.RequiredTargetKind, StringComparison.OrdinalIgnoreCase))
        {
            return Invalid(
                "CAMPAIGN_ACTION_TARGET_KIND_INVALID",
                "Campaign action target kind is not compatible with the action type.");
        }

        var parametersElement = actionConfig.GetProperty("parameters");
        if (parametersElement.ValueKind != JsonValueKind.Object)
        {
            return Invalid("CAMPAIGN_ACTION_PARAMETERS_INVALID", "Campaign action parameters must be an object.");
        }

        object? parsedParameters = null;
        if (actionDefinition.Code == ActionTypes.IssuePoint)
        {
            parsedParameters = ParseIssuePointParameters(parametersElement, actionDefinition, errors);
        }
        else
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_ACTION_TYPE_INVALID",
                $"Campaign action type '{actionDefinition.Code}' has no parameter parser."));
        }

        if (errors.Count > 0)
        {
            return new CampaignActionBindingParseResult(null, null, null, errors);
        }

        var canonicalJson = ToCanonicalJson(target.Selector, parsedParameters!);
        var canonicalBinding = JsonSerializer.Deserialize<CampaignActionBindingConfig>(
            canonicalJson,
            CanonicalJsonOptions);

        return new CampaignActionBindingParseResult(
            canonicalBinding,
            parsedParameters,
            canonicalJson,
            Array.Empty<CampaignDefinitionError>());
    }

    public static string ToCanonicalJson(string selector, object parameters)
    {
        var payload = new
        {
            target = new
            {
                selector
            },
            parameters
        };

        return JsonSerializer.Serialize(payload, CanonicalJsonOptions);
    }

    private static IssuePointParameters? ParseIssuePointParameters(
        JsonElement parametersElement,
        CampaignActionDefinition actionDefinition,
        List<CampaignDefinitionError> errors)
    {
        var parameterProperties = parametersElement.EnumerateObject()
            .Select(property => property.Name)
            .ToArray();
        if (parameterProperties.Length != 1 ||
            !parameterProperties.Contains("amount"))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_ACTION_PARAMETERS_INVALID",
                "ISSUE_POINT parameters must contain only amount."));
            return null;
        }

        var amountElement = parametersElement.GetProperty("amount");
        if (amountElement.ValueKind != JsonValueKind.Number ||
            !amountElement.TryGetDecimal(out var amount))
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_ACTION_AMOUNT_INVALID",
                "ISSUE_POINT amount must be a decimal number.",
                "parameters.amount"));
            return null;
        }

        var amountDefinition = actionDefinition.Parameters.First(parameter => parameter.Code == "amount");
        if (amountDefinition.MinimumExclusive is not null &&
            amount <= amountDefinition.MinimumExclusive.Value)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_ACTION_AMOUNT_INVALID",
                "ISSUE_POINT amount must be greater than zero.",
                "parameters.amount"));
        }

        if (amountDefinition.Maximum is not null &&
            amount > amountDefinition.Maximum.Value)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_ACTION_AMOUNT_INVALID",
                "ISSUE_POINT amount exceeds the maximum value.",
                "parameters.amount"));
        }

        if (amountDefinition.Scale is not null &&
            decimal.Round(amount, amountDefinition.Scale.Value) != amount)
        {
            errors.Add(new CampaignDefinitionError(
                "CAMPAIGN_ACTION_AMOUNT_INVALID",
                $"ISSUE_POINT amount supports at most {amountDefinition.Scale.Value} decimal places.",
                "parameters.amount"));
        }

        return errors.Count == 0 ? new IssuePointParameters(amount) : null;
    }

    private static CampaignActionBindingParseResult Invalid(string code, string message) =>
        new(null, null, null, [new CampaignDefinitionError(code, message)]);
}
