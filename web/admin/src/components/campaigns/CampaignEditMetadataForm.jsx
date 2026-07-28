import { useEffect, useState } from 'react'

import { Button } from '../ui/button'
import { CampaignMetadataFormFields } from './CampaignMetadataFormFields'

export function CampaignEditMetadataForm({
  initialValues = {},
  options = {},
  canUploadBanner = false,
  isSubmitting = false,
  formError = '',
  fieldErrors = {},
  onSubmit,
  onReset,
  t,
}) {
  const [formValues, setFormValues] = useState(initialValues)

  useEffect(() => {
    setFormValues(initialValues)
  }, [initialValues])

  function updateField(field, value) {
    setFormValues((prev) => ({ ...prev, [field]: value }))
  }

  function handleEventTypeChange(nextEventType) {
    const selectedEvent = (options.eventTypes || []).find((e) => e.value === nextEventType)
    const compatibleConditionPresets = (selectedEvent?.conditionPresets || []).map(
      (option) => option.value,
    )

    setFormValues((prev) => {
      const nextConditionPresetCode = compatibleConditionPresets.includes(
        prev.conditionPresetCode,
      )
        ? prev.conditionPresetCode
        : ''

      return {
        ...prev,
        eventType: nextEventType,
        conditionPresetCode: nextConditionPresetCode,
      }
    })
  }

  function handleConditionOptionChange(nextConditionPresetCode) {
    setFormValues((prev) => ({
      ...prev,
      conditionPresetCode: nextConditionPresetCode,
    }))
  }

  function handleReset() {
    setFormValues(initialValues)
    if (onReset) {
      onReset()
    }
  }

  function handleSubmit(event) {
    event.preventDefault()
    if (onSubmit) {
      onSubmit(formValues)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="grid gap-6">
      {formError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {formError}
        </div>
      ) : null}

      <CampaignMetadataFormFields
        formValues={formValues}
        options={options}
        canUploadBanner={canUploadBanner}
        isSubmitting={isSubmitting}
        fieldErrors={fieldErrors}
        updateField={updateField}
        handleEventTypeChange={handleEventTypeChange}
        handleConditionOptionChange={handleConditionOptionChange}
        t={t}
      />

      <div className="flex items-center justify-end gap-3 border-t border-border pt-6">
        <Button
          type="button"
          variant="outline"
          onClick={handleReset}
          disabled={isSubmitting}
        >
          {t('common.reset', { defaultValue: 'Reset' })}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? t('campaigns.form.saving', { defaultValue: 'Saving changes...' })
            : t('campaigns.form.saveChanges', { defaultValue: 'Save changes' })}
        </Button>
      </div>
    </form>
  )
}
