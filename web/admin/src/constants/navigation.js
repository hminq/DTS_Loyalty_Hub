import {
  ChartLineUpIcon,
  GiftIcon,
  HouseIcon,
  IdentificationBadgeIcon,
  TicketIcon,
  UsersThreeIcon,
  BellIcon,
} from '@phosphor-icons/react'

import { PermissionCodes } from './permissionCodes'

const navigationItems = [
  { id: 'dashboard', labelKey: 'navigation.dashboard', icon: HouseIcon, path: '/dashboard' },
  {
    id: 'role-permission',
    labelKey: 'navigation.rolePermissions',
    icon: IdentificationBadgeIcon,
    children: [
      { id: 'roles', labelKey: 'navigation.roles', path: '/roles', permission: PermissionCodes.Roles.View },
      { id: 'permissions', labelKey: 'navigation.permissions', path: '/permissions', permission: PermissionCodes.Permissions.View },
    ],
  },
  {
    id: 'accounts',
    labelKey: 'navigation.accounts',
    icon: UsersThreeIcon,
    children: [
      {
        id: 'admin-accounts',
        labelKey: 'navigation.admins',
        path: '/admin-accounts',
        permission: PermissionCodes.AdminUsers.View,
      },
      {
        id: 'customer-accounts',
        labelKey: 'navigation.customers',
        path: '/customer-accounts',
        permission: PermissionCodes.CustomerUsers.View,
      },
    ],
  },
  {
    id: 'voucher-definitions',
    labelKey: 'navigation.voucherDefinitions',
    icon: TicketIcon,
    path: '/voucher-definitions',
    permission: PermissionCodes.VoucherDefinitions.View,
  },
  {
    id: 'tiers',
    labelKey: 'navigation.tiers',
    icon: GiftIcon,
    path: '/tiers',
    permission: PermissionCodes.Tiers.View,
  },
  {
    id: 'audit-logs',
    labelKey: 'navigation.auditLogs',
    icon: ChartLineUpIcon,
    path: '/audit-logs',
    permission: PermissionCodes.AuditLogs.View,
  },
  {
    id: 'notification-templates',
    labelKey: 'navigation.notificationTemplates',
    icon: BellIcon,
    path: '/notification-templates',
    permission: PermissionCodes.NotificationTemplates.View,
  },
]

function getVisibleNavigation(items, permissionCodes) {
  return items
    .map((item) => {
      if (!item.children) {
        return item.permission && !permissionCodes.includes(item.permission) ? null : item
      }

      const visibleChildren = item.children.filter(
        (child) => !child.permission || permissionCodes.includes(child.permission),
      )

      return visibleChildren.length > 0 ? { ...item, children: visibleChildren } : null
    })
    .filter(Boolean)
}

export { getVisibleNavigation, navigationItems }
