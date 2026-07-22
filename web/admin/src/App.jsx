import { CircleNotchIcon } from '@phosphor-icons/react'
import { lazy, Suspense } from 'react'
import { useTranslation } from 'react-i18next'
import { Navigate, Route, Routes } from 'react-router-dom'

import { RequireAuth } from './components/auth/RequireAuth'
import { RequirePermission } from './components/auth/RequirePermission'
import { PermissionCodes } from './constants/permissionCodes'
import { AppLayout } from './layouts/AppLayout'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { FeaturePage } from './pages/FeaturePage'
import { PermissionsPage } from './pages/PermissionsPage'
import { RolesPage } from './pages/RolesPage'
import { RoleDetailPage } from './pages/RoleDetailPage'
import { CreateRolePage } from './pages/CreateRolePage'
import { EditRolePage } from './pages/EditRolePage'
import { SettingsPage } from './pages/SettingsPage'
import { SupportPage } from './pages/SupportPage'
import { AdminAccountsPage } from './pages/AdminAccountsPage'
import { AdminAccountDetailPage } from './pages/AdminAccountDetailPage'
import { CreateAdminAccountPage } from './pages/CreateAdminAccountPage'
import { NotificationTemplatesPage } from './pages/NotificationTemplatesPage'
import { NotificationTemplateDesignerPage } from './pages/NotificationTemplateDesignerPage'

const AdminAccountDetailPage = lazyNamed(() => import('./pages/AdminAccountDetailPage'), 'AdminAccountDetailPage')
const AdminAccountsPage = lazyNamed(() => import('./pages/AdminAccountsPage'), 'AdminAccountsPage')
const AuditLogsPage = lazyNamed(() => import('./pages/AuditLogsPage'), 'AuditLogsPage')
const CreateAdminAccountPage = lazyNamed(() => import('./pages/CreateAdminAccountPage'), 'CreateAdminAccountPage')
const CreateRolePage = lazyNamed(() => import('./pages/CreateRolePage'), 'CreateRolePage')
const DashboardPage = lazyNamed(() => import('./pages/DashboardPage'), 'DashboardPage')
const DesignSystemPage = lazyNamed(() => import('./pages/DesignSystemPage'), 'DesignSystemPage')
const EditRolePage = lazyNamed(() => import('./pages/EditRolePage'), 'EditRolePage')
const FeaturePage = lazyNamed(() => import('./pages/FeaturePage'), 'FeaturePage')
const LoginPage = lazyNamed(() => import('./pages/LoginPage'), 'LoginPage')
const NotFoundPage = lazyNamed(() => import('./pages/NotFoundPage'), 'NotFoundPage')
const PermissionsPage = lazyNamed(() => import('./pages/PermissionsPage'), 'PermissionsPage')
const RoleDetailPage = lazyNamed(() => import('./pages/RoleDetailPage'), 'RoleDetailPage')
const RolesPage = lazyNamed(() => import('./pages/RolesPage'), 'RolesPage')
const SettingsPage = lazyNamed(() => import('./pages/SettingsPage'), 'SettingsPage')
const SupportPage = lazyNamed(() => import('./pages/SupportPage'), 'SupportPage')

function App() {
  return (
    <Suspense fallback={<RouteLoading />}>
      <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />
      <Route
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route path="dashboard" element={<DashboardPage />} />
        <Route
          path="roles"
          element={
            <RequirePermission permission={PermissionCodes.Roles.View}>
              <RolesPage />
            </RequirePermission>
          }
        />
        <Route
          path="roles/new"
          element={
            <RequirePermission permissions={[
              PermissionCodes.Roles.View,
              PermissionCodes.Roles.Create,
            ]}>
              <CreateRolePage />
            </RequirePermission>
          }
        />
        <Route
          path="roles/:roleId/edit"
          element={
            <RequirePermission permissions={[
              PermissionCodes.Roles.View,
              PermissionCodes.Roles.Update,
            ]}>
              <EditRolePage />
            </RequirePermission>
          }
        />
        <Route
          path="roles/:roleId"
          element={
            <RequirePermission permission={PermissionCodes.Roles.View}>
              <RoleDetailPage />
            </RequirePermission>
          }
        />
        <Route
          path="permissions"
          element={
            <RequirePermission permission={PermissionCodes.Roles.View}>
              <PermissionsPage />
            </RequirePermission>
          }
        />
        <Route
          path="admin-accounts"
          element={
            <RequirePermission permission={PermissionCodes.AdminUsers.View}>
              <AdminAccountsPage />
            </RequirePermission>
          }
        />
        <Route
          path="admin-accounts/new"
          element={
            <RequirePermission permissions={[
              PermissionCodes.AdminUsers.View,
              PermissionCodes.AdminUsers.Create,
              PermissionCodes.Roles.View,
            ]}>
              <CreateAdminAccountPage />
            </RequirePermission>
          }
        />
        <Route
          path="admin-accounts/:adminId"
          element={
            <RequirePermission permission={PermissionCodes.AdminUsers.View}>
              <AdminAccountDetailPage />
            </RequirePermission>
          }
        />
        <Route path="admin-users" element={<Navigate to="/admin-accounts" replace />} />
        <Route
          path="customer-accounts"
          element={
            <RequirePermission permission={PermissionCodes.CustomerUsers.View}>
              <FeaturePage
                eyebrowKey="features.accounts.eyebrow"
                titleKey="features.customerUsers.title"
                descriptionKey="features.customerUsers.description"
              />
            </RequirePermission>
          }
        />
        <Route path="customer-users" element={<Navigate to="/customer-accounts" replace />} />
        <Route
          path="voucher-definitions"
          element={
            <RequirePermission permission={PermissionCodes.VoucherDefinitions.View}>
              <FeaturePage
                eyebrowKey="features.voucherDefinitions.eyebrow"
                titleKey="features.voucherDefinitions.title"
                descriptionKey="features.voucherDefinitions.description"
              />
            </RequirePermission>
          }
        />
        <Route
          path="tiers"
          element={
            <RequirePermission permission={PermissionCodes.Tiers.View}>
              <FeaturePage
                eyebrowKey="features.tiers.eyebrow"
                titleKey="features.tiers.title"
                descriptionKey="features.tiers.description"
              />
            </RequirePermission>
          }
        />
        <Route
          path="audit-logs"
          element={
            <RequirePermission permission={PermissionCodes.AuditLogs.View}>
              <AuditLogsPage />
            </RequirePermission>
          }
        />
        <Route
          path="notification-templates"
          element={
            <RequirePermission permission={PermissionCodes.NotificationTemplates.View}>
              <NotificationTemplatesPage />
            </RequirePermission>
          }
        />
        <Route
          path="notification-templates/new"
          element={
            <RequirePermission permissions={[
              PermissionCodes.NotificationTemplates.View,
              PermissionCodes.NotificationTemplates.Create,
            ]}>
              <NotificationTemplateDesignerPage />
            </RequirePermission>
          }
        />
        <Route
          path="notification-templates/:id"
          element={
            <RequirePermission permissions={[
              PermissionCodes.NotificationTemplates.View,
              PermissionCodes.NotificationTemplates.Update,
            ]}>
              <NotificationTemplateDesignerPage />
            </RequirePermission>
          }
        />
        <Route path="support" element={<SupportPage />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>
      <Route path="/design-system" element={<DesignSystemPage />} />
      <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  )
}

function RouteLoading() {
  const { t } = useTranslation()

  return (
    <main className="grid min-h-screen place-items-center bg-background text-foreground" aria-busy="true">
      <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
        <CircleNotchIcon className="animate-spin" />
        {t('common.loadingWorkspace')}
      </div>
    </main>
  )
}

function lazyNamed(loader, exportName) {
  return lazy(() => loader().then((module) => ({ default: module[exportName] })))
}

export default App
