import { useEffect, useState } from 'react'

import { Button } from '../ui/button'
import { Card, CardContent } from '../ui/card'
import { Field, FieldError, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { toFieldErrorMap } from '../../api/apiError'

function TierConfigForm({
  initialValues = { name: '', pointsRequired: 0, cycleMonth: 12, priority: 1 },
  isSubmitting = false,
  apiError = null,
  submitLabel,
  submittingLabel,
  onSubmit,
  onCancel,
  t,
}) {
  const [formValues, setFormValues] = useState({
    name: initialValues.name ?? '',
    pointsRequired: initialValues.pointsRequired ?? 0,
    cycleMonth: initialValues.cycleMonth ?? 12,
    priority: initialValues.priority ?? 1,
  })

  const [fieldErrors, setFieldErrors] = useState({})
  const [formLevelError, setFormLevelError] = useState('')

  useEffect(() => {
    setFormValues({
      name: initialValues?.name ?? '',
      pointsRequired: initialValues?.pointsRequired ?? 0,
      cycleMonth: initialValues?.cycleMonth ?? 12,
      priority: initialValues?.priority ?? 1,
    })
  }, [
    initialValues?.name,
    initialValues?.pointsRequired,
    initialValues?.cycleMonth,
    initialValues?.priority,
  ])

  useEffect(() => {
    if (!apiError) {
      setFieldErrors({})
      setFormLevelError('')
      return
    }

    const mapped = toFieldErrorMap(apiError.details)
    if (apiError.code === 'TIER_NAME_ALREADY_EXISTS') {
      mapped.name = apiError.message || t('tiers.errors.nameExists')
    }
    if (apiError.code === 'TIER_PRIORITY_ALREADY_EXISTS') {
      mapped.priority = apiError.message || t('tiers.errors.priorityExists')
    }

    setFieldErrors(mapped)

    if (apiError.code === 'TIER_POINTS_ORDER_INVALID' || (!apiError.details?.length && !mapped.name && !mapped.priority)) {
      setFormLevelError(apiError.message || t('tiers.errors.submitFailed'))
    } else {
      setFormLevelError('')
    }
  }, [apiError, t])

  function validate() {
    const errors = {}
    const trimmedName = formValues.name.trim()

    if (!trimmedName) {
      errors.name = t('tiers.validation.nameRequired')
    } else if (trimmedName.length < 3 || trimmedName.length > 49) {
      errors.name = t('tiers.validation.nameLength')
    }

    const points = Number(formValues.pointsRequired)
    if (Number.isNaN(points) || points < 0) {
      errors.pointsRequired = t('tiers.validation.pointsRequiredInvalid')
    }

    const cycle = Number(formValues.cycleMonth)
    if (Number.isNaN(cycle) || !Number.isInteger(cycle) || cycle < 1 || cycle > 12) {
      errors.cycleMonth = t('tiers.validation.cycleMonthInvalid')
    }

    const prio = Number(formValues.priority)
    if (Number.isNaN(prio) || !Number.isInteger(prio) || prio <= 0) {
      errors.priority = t('tiers.validation.priorityInvalid')
    }

    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  function handleSubmit(event) {
    event.preventDefault()
    setFormLevelError('')

    if (!validate()) return

    onSubmit({
      name: formValues.name.trim(),
      pointsRequired: Number(formValues.pointsRequired),
      cycleMonth: Number(formValues.cycleMonth),
      priority: Number(formValues.priority),
    })
  }

  function handleChange(field, value) {
    setFormValues((current) => ({ ...current, [field]: value }))
    if (fieldErrors[field]) {
      setFieldErrors((current) => ({ ...current, [field]: undefined }))
    }
  }

  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <CardContent className="p-6">
        <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-6">
          {formLevelError ? (
            <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
              {formLevelError}
            </div>
          ) : null}

          <div className="grid gap-6 sm:grid-cols-2">
            <Field invalid={Boolean(fieldErrors.name)}>
              <FieldLabel htmlFor="tier-name">{t('tiers.form.nameLabel')}</FieldLabel>
              <Input
                id="tier-name"
                value={formValues.name}
                onChange={(e) => handleChange('name', e.target.value)}
                placeholder={t('tiers.form.namePlaceholder')}
                disabled={isSubmitting}
                aria-invalid={Boolean(fieldErrors.name)}
              />
              {fieldErrors.name ? <FieldError>{fieldErrors.name}</FieldError> : null}
            </Field>

            <Field invalid={Boolean(fieldErrors.pointsRequired)}>
              <FieldLabel htmlFor="tier-points">{t('tiers.form.pointsRequiredLabel')}</FieldLabel>
              <Input
                id="tier-points"
                type="number"
                min="0"
                step="any"
                value={formValues.pointsRequired}
                onChange={(e) => handleChange('pointsRequired', e.target.value)}
                placeholder="0"
                disabled={isSubmitting}
                aria-invalid={Boolean(fieldErrors.pointsRequired)}
              />
              {fieldErrors.pointsRequired ? <FieldError>{fieldErrors.pointsRequired}</FieldError> : null}
            </Field>

            <Field invalid={Boolean(fieldErrors.cycleMonth)}>
              <FieldLabel htmlFor="tier-cycle">{t('tiers.form.cycleMonthLabel')}</FieldLabel>
              <Input
                id="tier-cycle"
                type="number"
                min="1"
                max="12"
                step="1"
                value={formValues.cycleMonth}
                onChange={(e) => handleChange('cycleMonth', e.target.value)}
                placeholder="12"
                disabled={isSubmitting}
                aria-invalid={Boolean(fieldErrors.cycleMonth)}
              />
              {fieldErrors.cycleMonth ? <FieldError>{fieldErrors.cycleMonth}</FieldError> : null}
            </Field>

            <Field invalid={Boolean(fieldErrors.priority)}>
              <FieldLabel htmlFor="tier-priority">{t('tiers.form.priorityLabel')}</FieldLabel>
              <Input
                id="tier-priority"
                type="number"
                min="1"
                step="1"
                value={formValues.priority}
                onChange={(e) => handleChange('priority', e.target.value)}
                placeholder="1"
                disabled={isSubmitting}
                aria-invalid={Boolean(fieldErrors.priority)}
              />
              {fieldErrors.priority ? <FieldError>{fieldErrors.priority}</FieldError> : null}
            </Field>
          </div>

          <div className="flex items-center justify-end gap-3 pt-2">
            <Button variant="outline" type="button" onClick={onCancel} disabled={isSubmitting}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? submittingLabel : submitLabel}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

export { TierConfigForm }
