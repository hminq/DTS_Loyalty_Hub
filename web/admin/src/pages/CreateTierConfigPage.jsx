import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { createTierConfig } from '../api/tiersApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { TierConfigForm } from '../components/tiers/TierConfigForm'

function CreateTierConfigPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [apiError, setApiError] = useState(null)

  async function handleSubmit(payload) {
    setIsSubmitting(true)
    setApiError(null)

    try {
      const createdTier = await createTierConfig(payload)
      const newId = createdTier?.tierConfigId
      navigate(newId ? `/tiers/${newId}` : '/tiers', {
        replace: true,
        state: { successMessage: t('tiers.create.success', { name: payload.name }) },
      })
    } catch (error) {
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
          { label: t('tiers.create.breadcrumb') },
        ]} />}
        title={t('tiers.create.title')}
        description={t('tiers.create.description')}
      />

      <TierConfigForm
        isSubmitting={isSubmitting}
        apiError={apiError}
        submitLabel={t('tiers.actions.create')}
        submittingLabel={t('tiers.create.submitting')}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/tiers')}
        t={t}
      />
    </>
  )
}

export { CreateTierConfigPage }
