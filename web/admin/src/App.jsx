import {
  ArrowRightIcon,
  BellIcon,
  CheckCircleIcon,
  MagnifyingGlassIcon,
  PlusIcon,
  UploadSimpleIcon,
} from '@phosphor-icons/react'
import { Navigate, Route, Routes } from 'react-router-dom'

import { Badge } from './components/ui/badge'
import { Button } from './components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './components/ui/card'
import { Input } from './components/ui/input'
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

const colorTokens = [
  ['Foreground', 'bg-foreground'],
  ['Primary', 'bg-primary'],
  ['Accent', 'bg-accent'],
  ['Muted', 'bg-muted'],
  ['Border', 'bg-border'],
]

function Section({ eyebrow, title, children }) {
  return (
    <section className="grid gap-6 border-t border-border py-10 md:grid-cols-[220px_1fr]">
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-primary">{eyebrow}</p>
        <h2 className="mt-2 text-lg font-semibold tracking-tight">{title}</h2>
      </div>
      <div>{children}</div>
    </section>
  )
}

function DesignSystemPage() {
  return (
    <main className="min-h-screen bg-background">
      <header className="border-b border-border">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4 lg:px-8">
          <div className="flex items-center gap-3">
            <div className="grid size-8 place-items-center rounded-md bg-foreground text-xs font-bold text-white">LH</div>
            <span className="text-sm font-semibold">Loyalty Hub Admin</span>
            <Badge variant="secondary">Design system</Badge>
          </div>
          <Button variant="ghost" size="icon" aria-label="Notifications">
            <BellIcon size={18} weight="bold" />
          </Button>
        </div>
      </header>

      <div className="mx-auto max-w-6xl px-6 py-14 lg:px-8">
        <div className="max-w-3xl">
          <Badge variant="outline">Foundation 01</Badge>
          <h1 className="mt-5 text-4xl font-semibold tracking-[-0.035em] sm:text-5xl">
            Clear, restrained interfaces for operational work.
          </h1>
          <p className="mt-5 max-w-2xl text-base leading-7 text-muted-foreground">
            A monochrome admin foundation with a deep blue accent for primary actions, focus, and active states.
          </p>
        </div>

        <div className="mt-14">
          <Section eyebrow="01 / Foundation" title="Color tokens">
            <div className="grid gap-3 sm:grid-cols-5">
              {colorTokens.map(([label, color]) => (
                <div key={label} className="rounded-lg border border-border p-2">
                  <div className={`h-20 rounded-md border border-black/5 ${color}`} />
                  <p className="px-1 pb-1 pt-3 text-xs font-medium">{label}</p>
                </div>
              ))}
            </div>
          </Section>

          <Section eyebrow="02 / Actions" title="Buttons">
            <div className="flex flex-wrap items-center gap-3">
              <Button><PlusIcon size={16} weight="bold" />Create voucher</Button>
              <Button variant="secondary">Secondary</Button>
              <Button variant="outline"><UploadSimpleIcon size={16} />Upload banner</Button>
              <Button variant="ghost">Ghost</Button>
              <Button variant="destructive">Delete</Button>
              <Button size="icon" aria-label="Continue"><ArrowRightIcon size={17} weight="bold" /></Button>
            </div>
          </Section>

          <Section eyebrow="03 / Forms" title="Inputs">
            <div className="grid max-w-2xl gap-5 sm:grid-cols-2">
              <label className="grid gap-2 text-sm font-medium">
                Voucher name
                <Input placeholder="Summer reward" />
                <span className="text-xs font-normal text-muted-foreground">Use a short, recognizable name.</span>
              </label>
              <label className="grid gap-2 text-sm font-medium">
                Search
                <div className="relative">
                  <MagnifyingGlassIcon className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" size={16} />
                  <Input className="pl-9" placeholder="Search voucher definitions" />
                </div>
              </label>
            </div>
          </Section>

          <Section eyebrow="04 / Status" title="Badges and cards">
            <div className="grid gap-5 lg:grid-cols-2">
              <Card>
                <CardHeader>
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <CardTitle>Voucher definition</CardTitle>
                      <CardDescription>Reusable surface for admin records.</CardDescription>
                    </div>
                    <Badge variant="success"><CheckCircleIcon size={13} weight="fill" />Active</Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="flex items-end justify-between rounded-lg bg-muted p-4">
                    <div>
                      <p className="text-xs text-muted-foreground">Remaining stock</p>
                      <p className="mt-1 text-2xl font-semibold tracking-tight">1,240</p>
                    </div>
                    <Button variant="outline" size="sm">View detail</Button>
                  </div>
                </CardContent>
              </Card>

              <Card className="bg-foreground text-white">
                <CardHeader>
                  <Badge className="w-fit bg-white/10 text-white">Dark surface</Badge>
                  <CardTitle className="pt-3 text-xl">Use dark blue sparingly.</CardTitle>
                  <CardDescription className="text-white/60">
                    Reserve accent color for intent, selection, and clear interaction feedback.
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Button className="bg-white text-foreground hover:bg-white/90">Review tokens<ArrowRightIcon size={16} /></Button>
                </CardContent>
              </Card>
            </div>
          </Section>
        </div>
      </div>
    </main>
  )
}

function App() {
  return (
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
              PermissionCodes.Permissions.View,
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
              PermissionCodes.Permissions.View,
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
            <RequirePermission permission={PermissionCodes.Permissions.View}>
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
              <FeaturePage
                eyebrowKey="features.auditLogs.eyebrow"
                titleKey="features.auditLogs.title"
                descriptionKey="features.auditLogs.description"
              />
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
  )
}

export default App
