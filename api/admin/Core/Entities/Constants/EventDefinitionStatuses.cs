namespace Core.Entities.Constants;

public static class EventDefinitionStatuses
{
    public const string Active = "ACTIVE";
    public const string Retired = "RETIRED";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Active,
        Retired
    ];

    public static bool IsDefined(string? status)
    {
        return status is not null && All.Contains(status, StringComparer.Ordinal);
    }
}
