import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { getPermissions } from '../api/permissionsApi'
import { PermissionMatrix } from '../components/permissions/PermissionMatrix'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { PageHeader } from '../components/layout/PageHeader'

function PermissionsPage() {
  const { t } = useTranslation()
  const [permissionGroups, setPermissionGroups] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadPermissions() {
      try {
        const groups = await getPermissions()

        if (isMounted) {
          setPermissionGroups(groups ?? [])
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error.message || t('errors.loadPermissions'))
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadPermissions()

    return () => {
      isMounted = false
    }
  }, [])

  return (
    <>
      <PageHeader
        eyebrow={t('permissions.eyebrow')}
        title={t('permissions.title')}
        description={t('permissions.description')}
      />

      <DataTableCard className="mt-6">
        {errorMessage ? (
          <p className="p-4 text-[13px] font-medium text-destructive">
            {errorMessage}
          </p>
        ) : null}

        {isLoading ? (
          <p className="p-4 text-[13px] text-muted-foreground">{t('permissions.loading')}</p>
        ) : permissionGroups.length === 0 ? (
          <p className="p-4 text-[13px] text-muted-foreground">{t('permissions.empty')}</p>
        ) : (
          <PermissionMatrix
            groups={permissionGroups}
            readOnly
            labels={{
              group: t('permissions.matrix.group'),
              defined: t('permissions.matrix.defined'),
              notDefined: t('permissions.matrix.notDefined'),
            }}
          />
        )}
      </DataTableCard>
    </>
  )
}

export { PermissionsPage }
