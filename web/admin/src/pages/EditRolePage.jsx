import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { getPermissions } from '../api/permissionsApi'
import { getRole, updateRole } from '../api/rolesApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { RoleForm } from '../components/roles/RoleForm'
import { PermissionCodes } from '../constants/permissionCodes'

function EditRolePage() {
  const { t } = useTranslation()
  const { roleId } = useParams()
  const navigate = useNavigate()
  const { refreshCurrentAdmin } = useOutletContext()
  const [role, setRole] = useState(null)
  const [permissionGroups, setPermissionGroups] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    let isCurrent = true

    async function loadData() {
      setRole(null)
      setPermissionGroups([])
      setErrorMessage('')
      setIsLoading(true)

      try {
        const [roleResult, groups] = await Promise.all([getRole(roleId), getPermissions()])
        if (!isCurrent) return
        setRole(roleResult)
        setPermissionGroups(groups ?? [])
      } catch (error) {
        if (isCurrent) setErrorMessage(error.message || t('errors.loadRole'))
      } finally {
        if (isCurrent) setIsLoading(false)
      }
    }

    loadData()
    return () => { isCurrent = false }
  }, [roleId, t])

  async function handleSubmit(payload) {
    await updateRole(roleId, payload)
    const refreshedAdmin = await refreshCurrentAdmin()
    const currentPermissions = refreshedAdmin?.permissions ?? []
    const stillHasEditRouteAccess = [
      PermissionCodes.Roles.View,
      PermissionCodes.Roles.Update,
    ].every((permission) => currentPermissions.includes(permission))

    if (!stillHasEditRouteAccess) {
      navigate('/dashboard', { replace: true })
      return
    }

    navigate(`/roles/${roleId}`, {
      replace: true,
      state: { successMessage: t('roles.editPage.success') },
    })
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('roles.title'), to: '/roles' },
          ...(role ? [{ label: role.name, to: `/roles/${role.roleId}` }] : []),
          { label: t('roles.editPage.titleFallback') },
        ]} />}
        title={role ? t('roles.editPage.title', { name: role.name }) : t('roles.editPage.titleFallback')}
        description={t('roles.editPage.description')}
      />
      {errorMessage ? <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">{errorMessage}</p> : null}
      {isLoading ? <p className="mt-5 text-[13px] text-muted-foreground">{t('roles.editPage.loading')}</p> : null}
      {role && !errorMessage ? (
        <RoleForm
          key={role.roleId}
          initialValues={{ name: role.name, permissionIds: role.permissionIds ?? [] }}
          permissionGroups={permissionGroups}
          submitLabel={t('roles.editPage.submit')}
          submittingLabel={t('roles.editPage.submitting')}
          onSubmit={handleSubmit}
          onCancel={() => navigate(`/roles/${role.roleId}`)}
        />
      ) : null}
    </>
  )
}

export { EditRolePage }
