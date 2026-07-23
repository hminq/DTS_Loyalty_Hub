import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import {
  getAdminAccount,
  revokeAdminAccountSession,
} from '../api/adminAccountsApi'
import { getPermissions } from '../api/permissionsApi'
import { AdminAccountDetails } from '../components/admin-accounts/AdminAccountDetails'
import { RevokeAdminSessionDialog } from '../components/admin-accounts/RevokeAdminSessionDialog'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function AdminAccountDetailPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()
  const { adminId } = useParams()
  const [account, setAccount] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState(location.state?.successMessage ?? '')
  const [revokeDialogOpen, setRevokeDialogOpen] = useState(false)
  const [permissionGroups, setPermissionGroups] = useState([])
  const [isPermissionsLoading, setIsPermissionsLoading] = useState(false)
  const [permissionErrorMessage, setPermissionErrorMessage] = useState('')

  const canEdit = hasPermission(PermissionCodes.AdminUsers.Update)
    && hasPermission(PermissionCodes.Roles.View)
  const canRevokeSession = hasPermission(PermissionCodes.AdminUsers.RevokeSession)

  useEffect(() => {
    if (location.state?.successMessage) {
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    const controller = new AbortController()

    async function loadAccount() {
      setIsLoading(true)
      setErrorMessage('')
      setAccount(null)
      setPermissionGroups([])
      setPermissionErrorMessage('')

      try {
        const result = await getAdminAccount(adminId, controller.signal)
        if (controller.signal.aborted) return
        setAccount(result)
      } catch (error) {
        if (!controller.signal.aborted) setErrorMessage(error.message || t('errors.loadAdminAccount'))
        return
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }

      setIsPermissionsLoading(true)
      try {
        const groups = await getPermissions()
        if (!controller.signal.aborted) setPermissionGroups(groups ?? [])
      } catch (error) {
        if (!controller.signal.aborted) {
          setPermissionErrorMessage(error.message || t('errors.loadPermissions'))
        }
      } finally {
        if (!controller.signal.aborted) setIsPermissionsLoading(false)
      }
    }

    loadAccount()
    return () => controller.abort()
  }, [adminId, t])

  async function handleRevokeSession() {
    await revokeAdminAccountSession(adminId)
    setRevokeDialogOpen(false)
    setSuccessMessage(t('adminAccounts.revoke.success'))
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('adminAccounts.title'), to: '/admin-accounts' },
          { label: account?.fullName || account?.username || t('adminAccounts.detail.titleFallback') },
        ]} />}
        title={account?.fullName || account?.username || t('adminAccounts.detail.titleFallback')}
        description={account ? `@${account.username}` : undefined}
        actions={account && (canEdit || canRevokeSession) ? (
          <>
            {canEdit ? (
              <Button variant="outline" size="sm" onClick={() => navigate(`/admin-accounts/${adminId}/edit`)}>
                {t('adminAccounts.actions.edit')}
              </Button>
            ) : null}
            {canRevokeSession ? (
              <Button variant="destructive" size="sm" onClick={() => setRevokeDialogOpen(true)}>
                {t('adminAccounts.actions.revokeSession')}
              </Button>
            ) : null}
          </>
        ) : null}
      />

      {successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </p>
      ) : null}
      {errorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {errorMessage}
        </p>
      ) : null}

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground">
          <CircleNotchIcon className="animate-spin" aria-hidden="true" />
          {t('adminAccounts.detail.loading')}
        </div>
      ) : account ? (
        <AdminAccountDetails
          account={account}
          language={i18n.resolvedLanguage}
          permissionGroups={permissionGroups}
          isPermissionsLoading={isPermissionsLoading}
          permissionErrorMessage={permissionErrorMessage}
          t={t}
        />
      ) : null}

      <RevokeAdminSessionDialog
        account={account}
        open={revokeDialogOpen}
        onClose={() => setRevokeDialogOpen(false)}
        onConfirm={handleRevokeSession}
      />
    </>
  )
}

export { AdminAccountDetailPage }
