import { PlusIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext } from 'react-router-dom'

import { getTierConfigs } from '../api/tiersApi'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { PageHeader } from '../components/layout/PageHeader'
import { TierConfigsTable } from '../components/tiers/TierConfigsTable'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function TierConfigsPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const outletContext = useOutletContext()
  const hasPermission = outletContext?.hasPermission || (() => false)

  const [tiers, setTiers] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const canCreate = hasPermission(PermissionCodes.Tiers.Create)
  const canEdit = hasPermission(PermissionCodes.Tiers.Update)

  useEffect(() => {
    if (location.state?.successMessage) {
      setSuccessMessage(location.state.successMessage)
      window.history.replaceState({}, document.title)
    }
    if (location.state?.errorMessage) {
      setErrorMessage(location.state.errorMessage)
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  const loadTiers = useCallback((signal) => {
    setIsLoading(true)
    setErrorMessage('')

    getTierConfigs(signal)
      .then((data) => {
        if (!signal?.aborted) setTiers(data ?? [])
      })
      .catch((error) => {
        if (signal?.aborted) return
        setErrorMessage(error.message || t('tiers.errors.loadCatalog'))
      })
      .finally(() => {
        if (!signal?.aborted) setIsLoading(false)
      })
  }, [t])

  useEffect(() => {
    const controller = new AbortController()
    loadTiers(controller.signal)
    return () => controller.abort()
  }, [loadTiers, refreshKey])

  return (
    <>
      <PageHeader
        eyebrow={t('tiers.eyebrow')}
        title={t('tiers.title')}
        description={t('tiers.description')}
        actions={
          canCreate ? (
            <Button size="sm" onClick={() => navigate('/tiers/new')}>
              <PlusIcon size={16} aria-hidden="true" />
              {t('tiers.actions.create')}
            </Button>
          ) : null
        }
      />

      {successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </p>
      ) : null}
      {errorMessage ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setRefreshKey((k) => k + 1)}
          >
            {t('common.retry')}
          </Button>
        </div>
      ) : null}

      <DataTableCard className="mt-5">
        <TierConfigsTable
          tiers={tiers}
          isLoading={isLoading}
          canEdit={canEdit}
          onView={(id) => navigate(`/tiers/${id}`)}
          onEdit={(id) => navigate(`/tiers/${id}/edit`)}
          language={i18n.resolvedLanguage}
          t={t}
        />
      </DataTableCard>
    </>
  )
}

export { TierConfigsPage }
