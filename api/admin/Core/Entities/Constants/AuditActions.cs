namespace Core.Entities.Constants;

public static class AuditActions
{
    public const string Create = "CREATE";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
    public const string UpdateStatus = "UPDATE_STATUS";
    public const string ToggleStatus = "TOGGLE_STATUS";
    public const string RevokeSession = "REVOKE_SESSION";
    public const string Import = "IMPORT";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Create,
        Update,
        Delete,
        UpdateStatus,
        ToggleStatus,
        RevokeSession,
        Import
    ];
}
