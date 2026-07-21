import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { getPermissions } from '../api/permissionsApi'
import { PermissionMatrix, humanizeAction } from '../components/permissions/PermissionMatrix'
import { PageHeader } from '../components/layout/PageHeader'
import { Card, CardContent } from '../components/ui/card'

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

      <Card className="mt-6 rounded-xl border-border/80 shadow-none">
        <CardContent className="p-4">
          {errorMessage ? (
            <p className="mb-3 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive">
              {errorMessage}
            </p>
          ) : null}

          {isLoading ? (
            <p className="text-[13px] text-muted-foreground">{t('permissions.loading')}</p>
          ) : permissionGroups.length === 0 ? (
            <p className="text-[13px] text-muted-foreground">{t('permissions.empty')}</p>
          ) : (
            <PermissionMatrix
              groups={permissionGroups}
              readOnly
              labels={{
                group: t('permissions.matrix.group'),
                defined: t('permissions.matrix.defined'),
                notDefined: t('permissions.matrix.notDefined'),
                action: (action) => t(`permissions.actions.${action}`, { defaultValue: humanizeAction(action) }),
              }}
            />
          )}
        </CardContent>
      </Card>
    </>
  )
}

export { PermissionsPage }
