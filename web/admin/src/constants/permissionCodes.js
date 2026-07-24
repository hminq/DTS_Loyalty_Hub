export const PermissionCodes = Object.freeze({
  Roles: Object.freeze({
    View: 'role.view',
    Create: 'role.create',
    Update: 'role.update',
    Delete: 'role.delete',
  }),
  AdminUsers: Object.freeze({
    View: 'admin_user.view',
    Create: 'admin_user.create',
    Update: 'admin_user.update',
    Disable: 'admin_user.disable',
    ResetPassword: 'admin_user.reset_password',
    RevokeSession: 'admin_user.revoke_session',
  }),
  CustomerUsers: Object.freeze({
    View: 'customer_user.view',
    Create: 'customer_user.create',
    Update: 'customer_user.update',
    Disable: 'customer_user.disable',
  }),
  Tiers: Object.freeze({
    View: 'tier.view',
    Create: 'tier.create',
    Update: 'tier.update',
  }),
  AuditLogs: Object.freeze({
    View: 'audit_log.view',
  }),
  NotificationEventTypes: Object.freeze({
    View: 'notification_event_type.view',
  }),
  NotificationTemplates: Object.freeze({
    View: 'notification_template.view',
    Create: 'notification_template.create',
    Update: 'notification_template.update',
  }),
  NotificationLogs: Object.freeze({
    View: 'notification_log.view',
  }),
  Media: Object.freeze({
    Upload: 'media.upload',
  }),
  VoucherDefinitions: Object.freeze({
    View: 'voucher_definition.view',
    Create: 'voucher_definition.create',
    Update: 'voucher_definition.update',
    Delete: 'voucher_definition.delete',
  }),
  CustomerVouchers: Object.freeze({
    View: 'customer_voucher.view',
  }),
})
