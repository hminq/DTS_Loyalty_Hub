import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { getPermissions } from '../api/permissionsApi'
import { deleteRole, getRole } from '../api/rolesApi'
import { DeleteRoleDialog } from '../components/roles/DeleteRoleDialog'
import { formatDateTime } from '../components/roles/RolesTable'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { PermissionMatrix } from '../components/permissions/PermissionMatrix'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card'
import { PermissionCodes } from '../constants/permissionCodes'

function RoleDetailPage() {
  const { i18n, t } = useTranslation()
  const { roleId } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()
  const [role, setRole] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [permissionGroups, setPermissionGroups] = useState([])
  const [isPermissionsLoading, setIsPermissionsLoading] = useState(false)
  const [permissionErrorMessage, setPermissionErrorMessage] = useState('')
  const canEdit = hasPermission(PermissionCodes.Roles.Update)
  const canDelete = hasPermission(PermissionCodes.Roles.Delete)

  useEffect(() => {
    let isCurrent = true

    async function loadRole() {
      setIsLoading(true)
      setErrorMessage('')
      setRole(null)
      setPermissionGroups([])
      setIsPermissionsLoading(false)
      setPermissionErrorMessage('')

      try {
        const roleResult = await getRole(roleId)
        if (!isCurrent) return
        setRole(roleResult)
      } catch (error) {
        if (isCurrent) setErrorMessage(error.message || t('errors.loadRole'))
        return
      } finally {
        if (isCurrent) setIsLoading(false)
      }

      setIsPermissionsLoading(true)
      try {
        const groups = await getPermissions()
        if (isCurrent) setPermissionGroups(groups ?? [])
      } catch (error) {
        if (isCurrent) setPermissionErrorMessage(error.message || t('errors.loadPermissions'))
      } finally {
        if (isCurrent) setIsPermissionsLoading(false)
      }
    }

    loadRole()
    return () => { isCurrent = false }
  }, [roleId, t])

  async function handleDelete() {
    try {
      await deleteRole(role.roleId)
    } catch (error) {
      if (error.code === 'ROLE_NOT_FOUND') {
        navigate('/roles', { replace: true, state: { errorMessage: error.message } })
        return
      }
      throw error
    }

    navigate('/roles', {
      replace: true,
      state: { successMessage: t('roles.delete.success', { name: role.name }) },
    })
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('roles.title'), to: '/roles' },
          { label: role?.name || t('roles.detail.titleFallback') },
        ]} />}
        title={role?.name || t('roles.detail.titleFallback')}
        description={t('roles.detail.description')}
        actions={role && (canEdit || canDelete) ? (
          <>
            {canEdit && role ? <Button variant="outline" size="sm" onClick={() => navigate(`/roles/${role.roleId}/edit`)}>{t('roles.actions.edit')}</Button> : null}
            {canDelete && role ? <Button variant="destructive" size="sm" onClick={() => setDeleteOpen(true)}>{t('roles.actions.delete')}</Button> : null}
          </>
        ) : null}
      />

      {location.state?.successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {location.state.successMessage}
        </p>
      ) : null}
      {errorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {errorMessage}
        </p>
      ) : null}
      {isLoading ? <p className="mt-5 text-[13px] text-muted-foreground">{t('roles.detail.loading')}</p> : null}

      {role ? (
        <Card className="mt-5 rounded-xl border-border/80 shadow-none">
          <section>
            <CardHeader><CardTitle>{t('roles.detail.identity')}</CardTitle></CardHeader>
            <CardContent className="grid gap-4 text-[13px] sm:grid-cols-3">
              <DetailItem label={t('roles.detail.name')} value={role.name} />
              <DetailItem label={t('roles.detail.id')} value={role.roleId} mono />
              <DetailItem label={t('roles.detail.createdAt')} value={formatDateTime(role.createdAt, i18n.resolvedLanguage)} />
            </CardContent>
          </section>

          <section className="border-t border-border">
            <CardHeader><CardTitle>{t('roles.detail.permissions')}</CardTitle></CardHeader>
            <CardContent>
              {isPermissionsLoading ? (
                <p className="text-[13px] text-muted-foreground">{t('permissions.loading')}</p>
              ) : permissionErrorMessage ? (
                <p className="text-[13px] font-medium text-destructive">{permissionErrorMessage}</p>
              ) : permissionGroups.length === 0 ? (
                <p className="text-[13px] text-muted-foreground">{t('permissions.empty')}</p>
              ) : (
                <PermissionMatrix
                  groups={permissionGroups}
                  selectedPermissionIds={role.permissionIds ?? []}
                  readOnly
                  labels={{
                    group: t('permissions.matrix.group'),
                    assigned: t('permissions.matrix.assigned'),
                    notAssigned: t('permissions.matrix.notAssigned'),
                    defined: t('permissions.matrix.defined'),
                    notDefined: t('permissions.matrix.notDefined'),
                  }}
                />
              )}
            </CardContent>
          </section>
        </Card>
      ) : null}

      <DeleteRoleDialog role={deleteOpen ? role : null} onClose={() => setDeleteOpen(false)} onConfirm={handleDelete} />
    </>
  )
}

function DetailItem({ label, value, mono = false }) {
  return (
    <div>
      <dt className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">{label}</dt>
      <dd className={mono ? 'mt-1 break-all font-mono text-xs' : 'mt-1 font-medium'}>{value}</dd>
    </div>
  )
}

export { RoleDetailPage }
