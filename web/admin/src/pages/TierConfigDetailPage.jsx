import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { getTierConfig } from '../api/tiersApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { TierConfigDetails } from '../components/tiers/TierConfigDetails'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function TierConfigDetailPage() {
  const { tierConfigId } = useParams()
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const outletContext = useOutletContext()
  const hasPermission = outletContext?.hasPermission || (() => false)

  const [tier, setTier] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const canEdit = hasPermission(PermissionCodes.Tiers.Update)

  useEffect(() => {
    if (location.state?.successMessage) {
      setSuccessMessage(location.state.successMessage)
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    const controller = new AbortController()
    setIsLoading(true)
    setErrorMessage('')
    setTier(null)

    getTierConfig(tierConfigId, controller.signal)
      .then((data) => {
        if (!controller.signal.aborted) setTier(data)
      })
      .catch((error) => {
        if (controller.signal.aborted) return

        if (error.code === 'TIER_NOT_FOUND') {
          navigate('/tiers', {
            replace: true,
            state: { errorMessage: t('tiers.errors.notFound') },
          })
          return
        }

        setErrorMessage(error.message || t('tiers.errors.loadDetail'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [tierConfigId, refreshKey, navigate, t])

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('tiers.title'), to: '/tiers' },
          { label: tier?.name || t('tiers.detail.titleFallback') },
        ]} />}
        title={tier?.name || t('tiers.detail.titleFallback')}
        description={t('tiers.detail.description')}
        actions={
          canEdit && tier ? (
            <Button variant="outline" size="sm" onClick={() => navigate(`/tiers/${tierConfigId}/edit`)}>
              {t('tiers.actions.edit')}
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

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('tiers.loading')}
        </div>
      ) : tier ? (
        <TierConfigDetails tier={tier} language={i18n.resolvedLanguage} t={t} />
      ) : null}
    </>
  )
}

export { TierConfigDetailPage }
