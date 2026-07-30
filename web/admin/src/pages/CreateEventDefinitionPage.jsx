import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { createEventDefinition, getEventDefinitionOptions } from '../api/eventDefinitionsApi'
import { EventDefinitionMetadataForm } from '../components/event-definitions/EventDefinitionMetadataForm'
import { EventDefinitionSchemaBuilder } from '../components/event-definitions/EventDefinitionSchemaBuilder'
import { mapEventDefinitionOptions } from '../components/event-definitions/eventDefinitionOptions'
import { buildCreateEventDefinitionPayload } from '../components/event-definitions/eventDefinitionPayloads'
import { validateMetadataForm, validateSchemaForm } from '../components/event-definitions/eventDefinitionValidation'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { toFieldErrorMap } from '../api/apiError'

export function CreateEventDefinitionPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const [optionsData, setOptionsData] = useState({})
  const [metadata, setMetadata] = useState({
    code: '',
    routingKey: '',
    name: '',
    description: '',
  })
  const [fields, setFields] = useState([])
  const [targets, setTargets] = useState([])

  const [metadataErrors, setMetadataErrors] = useState({})
  const [fieldErrors, setFieldErrors] = useState([])
  const [targetErrors, setTargetErrors] = useState([])
  const [apiError, setApiError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const options = useMemo(() => mapEventDefinitionOptions(optionsData, t), [optionsData, t])

  useEffect(() => {
    getEventDefinitionOptions()
      .then((opts) => setOptionsData(opts || {}))
      .catch(() => {})
  }, [])

  const handleMetadataChange = (key, value) => {
    setMetadata((prev) => ({ ...prev, [key]: value }))
    if (metadataErrors[key]) {
      setMetadataErrors((prev) => ({ ...prev, [key]: null }))
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setApiError(null)

    const metaVal = validateMetadataForm(metadata, { canEditIdentity: true })
    const schemaVal = validateSchemaForm(fields, targets)

    setMetadataErrors(metaVal.errors)
    setFieldErrors(schemaVal.fieldErrors)
    setTargetErrors(schemaVal.targetErrors)

    if (!metaVal.isValid || !schemaVal.isValid) {
      return
    }

    setIsSubmitting(true)

    try {
      const payload = buildCreateEventDefinitionPayload(metadata, fields, targets)
      const res = await createEventDefinition(payload)

      if (res?.eventTypeId) {
        navigate(`/event-definitions/${res.eventTypeId}`, {
          state: { notice: t('eventDefinitions.notices.createdSuccess') },
        })
      }
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

  const breadcrumbItems = [
    { label: t('eventDefinitions.title'), to: '/event-definitions' },
    { label: t('eventDefinitions.create') },
  ]

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-6" noValidate>
      <PageHeader
        breadcrumb={<Breadcrumb items={breadcrumbItems} />}
        title={t('eventDefinitions.createTitle')}
        description={t('eventDefinitions.createDescription')}
      />

      {apiError && (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {apiError}
        </div>
      )}

      <EventDefinitionMetadataForm
        values={metadata}
        onChange={handleMetadataChange}
        errors={metadataErrors}
        canEditIdentity={true}
        disabled={isSubmitting}
      />

      <EventDefinitionSchemaBuilder
        fields={fields}
        targets={targets}
        fieldTypeOptions={options.fieldTypeOptions}
        targetKindOptions={options.targetKindOptions}
        onChangeFields={setFields}
        onChangeTargets={setTargets}
        fieldErrors={fieldErrors}
        targetErrors={targetErrors}
        disabled={isSubmitting}
      />

      <div className="flex items-center justify-end gap-3 pt-2">
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => navigate('/event-definitions')}
          disabled={isSubmitting}
        >
          {t('common.cancel')}
        </Button>
        <Button type="submit" size="sm" disabled={isSubmitting}>
          {isSubmitting ? t('common.creating') : t('common.create')}
        </Button>
      </div>
    </form>
  )
}
