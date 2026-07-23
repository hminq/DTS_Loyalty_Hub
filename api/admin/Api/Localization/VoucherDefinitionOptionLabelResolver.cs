using Microsoft.Extensions.Localization;

namespace Api.Localization;

public sealed class VoucherDefinitionOptionLabelResolver
{
    private readonly IStringLocalizer<VoucherDefinitionOptions> _localizer;

    public VoucherDefinitionOptionLabelResolver(IStringLocalizer<VoucherDefinitionOptions> localizer)
    {
        _localizer = localizer;
    }

    public string ResolveRewardType(string value) => Resolve("REWARD_TYPE", value);
    public string ResolveValidityType(string value) => Resolve("VALIDITY_TYPE", value);
    public string ResolvePublishType(string value) => Resolve("PUBLISH_TYPE", value);
    public string ResolveGenerationType(string value) => Resolve("GENERATION_TYPE", value);

    private string Resolve(string groupPrefix, string value)
    {
        var key = $"{groupPrefix}_{value.ToUpperInvariant()}";
        var localizedString = _localizer[key];
        
        return localizedString.ResourceNotFound ? value : localizedString.Value;
    }
}
