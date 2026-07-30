namespace Core.Entities.Constants;

public static class EventDefinitionSchemaLimits
{
    public const int MaximumFields = 100;
    public const int MaximumTargets = 20;
    public const int MaximumSchemaBytes = 65_536;
    public const int MaximumCodeLength = 100;
    public const int MaximumRoutingKeyLength = 255;
    public const int MaximumNameLength = 200;
    public const int MaximumDescriptionLength = 2_000;
    public const int MaximumKeywordLength = 100;
}
