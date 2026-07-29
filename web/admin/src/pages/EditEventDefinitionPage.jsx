import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'

import { getEventDefinition, updateEventDefinition } from '../api/eventDefinitionsApi'
import { EventDefinitionMetadataForm } from '../components/event-definitions/EventDefinitionMetadataForm'
import { mapDetailToMetadataForm, buildUpdateMetadataPayload } from '../components/event-definitions/eventDefinitionPayloads'
import { validateMetadataForm } from '../components/event-definitions/eventDefinitionValidation'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { toFieldErrorMap } from '../api/apiError'

export function EditEventDefinitionPage() {
  const { t } = useTranslation()
  const { eventTypeId } = useParams()
  const navigate = useNavigate()

  const [detail, setDetail] = useState(null)
  const [metadata, setMetadata] = useState({
    code: '',
    routingKey: '',
    name: '',
    description: '',
  })
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [metadataErrors, setMetadataErrors] = useState({})
  const [apiError, setApiError] = useState(null)

  useEffect(() => {
    if (!eventTypeId) return
    setIsLoading(true)
    getEventDefinition(eventTypeId)
      .then((data) => {
        setDetail(data)
        setMetadata(mapDetailToMetadataForm(data))
      })
      .catch((err) => setApiError(err?.message || t('errors.unexpected')))
      .finally(() => setIsLoading(false))
  }, [eventTypeId, t])

  const handleMetadataChange = (key, value) => {
    setMetadata((prev) => ({ ...prev, [key]: value }))
    if (metadataErrors[key]) {
      setMetadataErrors((prev) => ({ ...prev, [key]: null }))
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setApiError(null)

    const canEditIdentity = Boolean(detail?.canEditIdentity)
    const metaVal = validateMetadataForm(metadata, { canEditIdentity })
    setMetadataErrors(metaVal.errors)

    if (!metaVal.isValid) return

    setIsSubmitting(true)
    try {
      const payload = buildUpdateMetadataPayload(metadata)
      await updateEventDefinition(eventTypeId, payload)
      navigate(`/event-definitions/${eventTypeId}`, {
        state: { notice: t('eventDefinitions.notices.updatedMetadataSuccess') },
      })
    } catch (err) {
      const responseError = err?.response?.data?.error
      if (responseError?.details && Array.isArray(responseError.details)) {
        const mappedFieldErrors = toFieldErrorMap(responseError.details)
        setMetadataErrors((prev) => ({ ...prev, ...mappedFieldErrors }))
      }
      setApiError(responseError?.message || err?.message || t('errors.unexpected'))
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-12 text-muted-foreground">
        <CircleNotchIcon className="mr-2 animate-spin" size={20} />
        {t('eventDefinitions.loadingDetail')}
      </div>
    )
  }

  if (apiError && !detail) {
    return (
      <div className="rounded-md bg-destructive/10 p-4 text-sm font-medium text-destructive">
        {apiError}
      </div>
    )
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PageHeader
        title={t('eventDefinitions.editTitle', { name: detail?.name })}
        description={detail?.code}
        actions={
          <div className="flex items-center gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate(`/event-definitions/${eventTypeId}`)}
              disabled={isSubmitting}
            >
              {t('common.cancel')}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? (
                <span className="inline-flex items-center gap-1.5">
                  <CircleNotchIcon className="animate-spin" size={14} />
                  {t('common.saving', { defaultValue: 'Saving...' })}
                </span>
              ) : (
                t('common.save', { defaultValue: 'Save' })
              )}
            </Button>
          </div>
        }
      />

      {apiError && (
        <div className="rounded-md bg-destructive/10 p-4 text-sm font-medium text-destructive">
          {apiError}
        </div>
      )}

      <EventDefinitionMetadataForm
        values={metadata}
        onChange={handleMetadataChange}
        errors={metadataErrors}
        canEditIdentity={Boolean(detail?.canEditIdentity)}
        disabled={isSubmitting}
      />
    </form>
  )
}
