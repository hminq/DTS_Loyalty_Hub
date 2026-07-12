namespace Core.Entities;

public static class PermissionCodes
{
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
}
