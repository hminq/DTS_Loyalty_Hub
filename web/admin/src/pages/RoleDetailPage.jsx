import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { deleteRole, getRole } from '../api/rolesApi'
import { DeleteRoleDialog } from '../components/roles/DeleteRoleDialog'
import { formatDateTime } from '../components/roles/RolesTable'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
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
  const permissionGroups = useMemo(() => groupPermissions(role?.permissions ?? []), [role])
  const canEdit = hasPermission(PermissionCodes.Roles.Update)
    && hasPermission(PermissionCodes.Permissions.View)
  const canDelete = hasPermission(PermissionCodes.Roles.Delete)

  useEffect(() => {
    let isCurrent = true

    async function loadRole() {
      try {
        const result = await getRole(roleId)
        if (isCurrent) setRole(result)
      } catch (error) {
        if (isCurrent) setErrorMessage(error.message || t('errors.loadRole'))
      } finally {
        if (isCurrent) setIsLoading(false)
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
        <div className="mt-5 grid gap-4 xl:grid-cols-[minmax(0,0.75fr)_minmax(0,1.25fr)]">
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader><CardTitle>{t('roles.detail.identity')}</CardTitle></CardHeader>
            <CardContent className="grid gap-4 text-[13px]">
              <DetailItem label={t('roles.detail.name')} value={role.name} />
              <DetailItem label={t('roles.detail.id')} value={role.roleId} mono />
              <DetailItem label={t('roles.detail.createdAt')} value={formatDateTime(role.createdAt, i18n.resolvedLanguage)} />
            </CardContent>
          </Card>

          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader><CardTitle>{t('roles.detail.permissions')}</CardTitle></CardHeader>
            <CardContent className="flex flex-col gap-3">
              {permissionGroups.length === 0 ? <p className="text-[13px] text-muted-foreground">{t('roles.detail.noPermissions')}</p> : permissionGroups.map((group) => (
                <section key={group.groupCode} className="rounded-lg border border-border p-3">
                  <div className="flex flex-wrap items-baseline justify-between gap-2">
                    <h3 className="text-xs font-semibold">{group.groupName}</h3>
                    <code className="text-[11px] text-muted-foreground">{group.groupCode}</code>
                  </div>
                  <div className="mt-3 grid gap-2">
                    {group.permissions.map((permission) => (
                      <div key={permission.permissionId} className="rounded-md bg-muted/55 px-3 py-2">
                        <p className="text-xs font-medium">{permission.name}</p>
                        <code className="mt-0.5 block text-[11px] text-muted-foreground">{permission.code}</code>
                      </div>
                    ))}
                  </div>
                </section>
              ))}
            </CardContent>
          </Card>
        </div>
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

function groupPermissions(permissions) {
  const groups = new Map()

  permissions.forEach((permission) => {
    if (!groups.has(permission.groupCode)) {
      groups.set(permission.groupCode, {
        groupCode: permission.groupCode,
        groupName: permission.groupName,
        sortOrder: permission.groupSortOrder,
        permissions: [],
      })
    }
    groups.get(permission.groupCode).permissions.push(permission)
  })

  return [...groups.values()]
    .sort((left, right) => left.sortOrder - right.sortOrder || left.groupName.localeCompare(right.groupName))
    .map((group) => ({
      ...group,
      permissions: group.permissions.sort((left, right) =>
        left.actionSortOrder - right.actionSortOrder || left.code.localeCompare(right.code)),
    }))
}

export { RoleDetailPage }
