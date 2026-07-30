using System.Text.Json;

namespace Campaign.Contracts.Actions;

public sealed record CampaignActionBindingConfig(
    CampaignActionTargetConfig Target,
    JsonElement Parameters);
