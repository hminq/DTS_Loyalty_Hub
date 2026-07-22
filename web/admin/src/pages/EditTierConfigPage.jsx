import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'

import { getTierConfig, updateTierConfig } from '../api/tiersApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { TierConfigForm } from '../components/tiers/TierConfigForm'
import { Button } from '../components/ui/button'

function EditTierConfigPage() {
  const { tierConfigId } = useParams()
  const { t } = useTranslation()
  const navigate = useNavigate()

  const [tier, setTier] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [apiError, setApiError] = useState(null)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    const controller = new AbortController()
    setIsLoading(true)
    setLoadError('')

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

        setLoadError(error.message || t('tiers.errors.loadDetail'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [tierConfigId, refreshKey, navigate, t])

  async function handleSubmit(payload) {
    setIsSubmitting(true)
    setApiError(null)

    try {
      await updateTierConfig(tierConfigId, payload)
      navigate(`/tiers/${tierConfigId}`, {
        replace: true,
        state: { successMessage: t('tiers.edit.success', { name: payload.name }) },
      })
    } catch (error) {
      if (error.code === 'TIER_NOT_FOUND') {
        navigate('/tiers', {
          replace: true,
          state: { errorMessage: t('tiers.errors.notFound') },
        })
        return
      }
      setApiError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('tiers.title'), to: '/tiers' },
          { label: tier?.name || t('tiers.detail.titleFallback'), to: `/tiers/${tierConfigId}` },
          { label: t('tiers.edit.breadcrumb') },
        ]} />}
        title={t('tiers.edit.title')}
        description={t('tiers.edit.description')}
      />

      {loadError ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{loadError}</p>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setRefreshKey((k) => k + 1)}
          >
            {t('common.retry')}
          </Button>
        </div>
      ) : isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('tiers.loading')}
        </div>
      ) : tier ? (
        <TierConfigForm
          initialValues={{
            name: tier.name,
            pointsRequired: tier.pointsRequired,
            cycleMonth: tier.cycleMonth,
            priority: tier.priority,
          }}
          isSubmitting={isSubmitting}
          apiError={apiError}
          submitLabel={t('tiers.actions.save')}
          submittingLabel={t('tiers.edit.submitting')}
          onSubmit={handleSubmit}
          onCancel={() => navigate(`/tiers/${tierConfigId}`)}
          t={t}
        />
      ) : null}
    </>
  )
}

export { EditTierConfigPage }
