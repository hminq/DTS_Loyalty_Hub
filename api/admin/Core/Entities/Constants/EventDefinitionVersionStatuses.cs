namespace Core.Entities.Constants;

public static class EventDefinitionVersionStatuses
{
    public const string Draft = "DRAFT";
    public const string Published = "PUBLISHED";
    public const string Retired = "RETIRED";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Draft,
        Published,
        Retired
    ];
}
