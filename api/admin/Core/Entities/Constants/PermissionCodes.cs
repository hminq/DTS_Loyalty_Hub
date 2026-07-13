namespace Core.Entities.Constants;

public static class PermissionCodes
{
    private static readonly HashSet<string> DefinedCodes =
    [
        Roles.View,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Permissions.View,
        RolePermissions.View,
        RolePermissions.Update
    ];

    public static IReadOnlySet<string> All => DefinedCodes;

    public static bool IsDefined(string code)
    {
        return DefinedCodes.Contains(Normalize(code));
    }

    public static string GetGroupCode(string code)
    {
        var normalizedCode = Normalize(code);
        var separatorIndex = normalizedCode.IndexOf('.', StringComparison.Ordinal);

        return separatorIndex < 0
            ? normalizedCode
            : normalizedCode[..separatorIndex];
    }

    public static class Roles
    {
        public const string View = "role.view";
        public const string Create = "role.create";
        public const string Update = "role.update";
        public const string Delete = "role.delete";
    }

    public static class Permissions
    {
        public const string View = "permission.view";
    }

    public static class RolePermissions
    {
        public const string View = "role_permission.view";
        public const string Update = "role_permission.update";
    }

    private static string Normalize(string code)
    {
        return code.Trim().ToLowerInvariant();
    }
}
