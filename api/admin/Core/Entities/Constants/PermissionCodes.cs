namespace Core.Entities.Constants;

public static class PermissionCodes
{
    private static readonly HashSet<string> DefinedCodes =
    [
        Roles.View,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        AdminUsers.View,
        AdminUsers.Create,
        AdminUsers.Update,
        AdminUsers.Disable,
        AdminUsers.ResetPassword,
        AdminUsers.RevokeSession,
        CustomerUsers.View,
        CustomerUsers.Create,
        CustomerUsers.Update,
        CustomerUsers.Disable,
        CustomerUsers.ResetPassword,
        Tiers.View,
        Tiers.Create,
        AuditLogs.View,
        NotificationEventTypes.View,
        NotificationTemplates.View,
        NotificationTemplates.Create,
        NotificationTemplates.Update,
        NotificationLogs.View,
        Media.Upload,
        VoucherDefinitions.View,
        VoucherDefinitions.Create,
        VoucherDefinitions.Update,
        VoucherDefinitions.Delete,
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

    public static class AdminUsers
    {
        public const string View = "admin_user.view";
        public const string Create = "admin_user.create";
        public const string Update = "admin_user.update";
        public const string Disable = "admin_user.disable";
        public const string ResetPassword = "admin_user.reset_password";
        public const string RevokeSession = "admin_user.revoke_session";
    }

    public static class CustomerUsers
    {
        public const string View = "customer_user.view";
        public const string Create = "customer_user.create";
        public const string Update = "customer_user.update";
        public const string Disable = "customer_user.disable";
        public const string ResetPassword = "customer_user.reset_password";
    }

    public static class VoucherDefinitions
    {
        public const string View = "voucher_definition.view";
        public const string Create = "voucher_definition.create";
        public const string Update = "voucher_definition.update";
        public const string Delete = "voucher_definition.delete";
    }

    private static string Normalize(string code)
    {
        return code.Trim().ToLowerInvariant();
    }

    public static class Tiers
    {
        public const string View = "tier.view";
        public const string Create = "tier.create";
    }

    public static class AuditLogs
    {
        public const string View = "audit_log.view";
    }

    public static class NotificationEventTypes
    {
        public const string View = "notification_event_type.view";
    }

    public static class NotificationTemplates
    {
        public const string View = "notification_template.view";
        public const string Create = "notification_template.create";
        public const string Update = "notification_template.update";
    }

    public static class NotificationLogs
    {
        public const string View = "notification_log.view";
    }

    public static class Media
    {
        public const string Upload = "media.upload";
    }
}
