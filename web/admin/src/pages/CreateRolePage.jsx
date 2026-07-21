import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { getPermissions } from '../api/permissionsApi'
import { createRole } from '../api/rolesApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { RoleForm } from '../components/roles/RoleForm'

function CreateRolePage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [permissionGroups, setPermissionGroups] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    let isCurrent = true

    async function loadPermissions() {
      try {
        const groups = await getPermissions()
        if (isCurrent) setPermissionGroups(groups ?? [])
      } catch (error) {
        if (isCurrent) setErrorMessage(error.message || t('errors.loadPermissions'))
      } finally {
        if (isCurrent) setIsLoading(false)
      }
    }

    loadPermissions()
    return () => { isCurrent = false }
  }, [t])

  async function handleSubmit(payload) {
    const createdRole = await createRole(payload)
    navigate(`/roles/${createdRole.roleId}`, {
      replace: true,
      state: { successMessage: t('roles.createPage.success') },
    })
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('roles.title'), to: '/roles' },
          { label: t('roles.createPage.title') },
        ]} />}
        title={t('roles.createPage.title')}
        description={t('roles.createPage.description')}
      />
      {errorMessage ? <ErrorMessage message={errorMessage} /> : null}
      {isLoading ? <p className="mt-5 text-[13px] text-muted-foreground">{t('permissions.loading')}</p> : null}
      {!isLoading && !errorMessage ? (
        <RoleForm
          initialValues={{ name: '', permissionIds: [] }}
          permissionGroups={permissionGroups}
          submitLabel={t('roles.createPage.submit')}
          submittingLabel={t('roles.createPage.submitting')}
          onSubmit={handleSubmit}
          onCancel={() => navigate('/roles')}
        />
      ) : null}
    </>
  )
}

function ErrorMessage({ message }) {
  return <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">{message}</p>
}

export { CreateRolePage }
