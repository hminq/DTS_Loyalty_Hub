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
        Tiers.View,
        Tiers.Create,
        Tiers.Update,
        AuditLogs.View,
        Notifications.View,
        Notifications.Create,
        Notifications.Update,
        Notifications.Delete,
        Media.Upload,
        VoucherDefinitions.View,
        VoucherDefinitions.Create,
        VoucherDefinitions.Update,
        VoucherDefinitions.Delete,
        Campaigns.View,
        Campaigns.Create,
        Campaigns.Update,
        Campaigns.Delete,
        EventDefinitions.View,
        EventDefinitions.Create,
        EventDefinitions.Update,
        CustomerVouchers.View
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
    }

    public static class VoucherDefinitions
    {
        public const string View = "voucher_definition.view";
        public const string Create = "voucher_definition.create";
        public const string Update = "voucher_definition.update";
        public const string Delete = "voucher_definition.delete";
    }

    public static class Campaigns
    {
        public const string View = "campaign.view";
        public const string Create = "campaign.create";
        public const string Update = "campaign.update";
        public const string Delete = "campaign.delete";
    }

    public static class EventDefinitions
    {
        public const string View = "event_definition.view";
        public const string Create = "event_definition.create";
        public const string Update = "event_definition.update";
    }

    private static string Normalize(string code)
    {
        return code.Trim().ToLowerInvariant();
    }

    public static class Tiers
    {
        public const string View = "tier.view";
        public const string Create = "tier.create";
        public const string Update = "tier.update";
    }

    public static class AuditLogs
    {
        public const string View = "audit_log.view";
    }

    public static class CustomerVouchers
    {
        public const string View = "customer_voucher.view";
    }

    public static class Notifications
    {
        public const string View = "notification.view";
        public const string Create = "notification.create";
        public const string Update = "notification.update";
        public const string Delete = "notification.delete";
    }

    public static class NotificationTemplates
    {
        public const string View = Notifications.View;
        public const string Create = Notifications.Create;
        public const string Update = Notifications.Update;
        public const string Delete = Notifications.Delete;
    }

    public static class NotificationLogs
    {
        public const string View = Notifications.View;
    }

    public static class Media
    {
        public const string Upload = "media.upload";
    }
}
