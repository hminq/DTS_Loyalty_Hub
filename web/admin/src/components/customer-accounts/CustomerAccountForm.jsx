import { useId, useState } from 'react'

import { toFieldErrorMap } from '../../api/apiError'
import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { Card, CardContent } from '../ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'

const emptyForm = Object.freeze({
  username: '',
  email: '',
  fullName: '',
  phoneNumber: '',
  tierName: '',
  status: '',
})

function CustomerAccountForm({
  initialValues = emptyForm,
  submitLabel,
  submittingLabel,
  submitError,
  onSubmit,
  onCancel,
  t,
}) {
  const formId = useId()
  const [form, setForm] = useState(() => ({ ...emptyForm, ...initialValues }))
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
    clearFieldError(field)
  }

  function clearFieldError(field) {
    setFieldErrors((current) => {
      if (!current[field]) return current
      const next = { ...current }
      delete next[field]
      return next
    })
  }

  async function handleSubmit(event) {
    event.preventDefault()
    const validationErrors = validateForm(form, t)

    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors)
      return
    }

    setIsSubmitting(true)
    setFieldErrors({})
    setFormError('')

    try {
      await onSubmit({
        email: form.email.trim(),
        fullName: form.fullName.trim() || null,
        phoneNumber: form.phoneNumber.trim() || null,
      })
    } catch (error) {
      setFieldErrors(toFieldErrorMap(error.details))
      setFormError(error.message || submitError)
      setIsSubmitting(false)
    }
  }

  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <CardContent className="p-5">
        <form onSubmit={handleSubmit} noValidate>
          <FieldGroup className="sm:grid-cols-2">
            {formError ? (
              <p className="rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive sm:col-span-2">
                {formError}
              </p>
            ) : null}

            <div className="rounded-lg border border-border/60 bg-muted/20 p-4.5 sm:col-span-2">
              <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                {t('customerAccounts.edit.summaryTitle')}
              </h3>
              <div className="mt-3 grid gap-4 text-[13px] sm:grid-cols-3">
                <div>
                  <span className="text-muted-foreground">{t('customerAccounts.detail.username')}</span>
                  <p className="mt-0.5 font-medium text-foreground">@{form.username || '-'}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">{t('customerAccounts.detail.tierName')}</span>
                  <p className="mt-0.5 font-medium text-foreground">{form.tierName || t('customerAccounts.unassignedTier')}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">{t('customerAccounts.detail.status')}</span>
                  <div className="mt-1">
                    <Badge variant={form.status === 'ENABLE' ? 'success' : 'secondary'}>
                      {form.status === 'ENABLE'
                        ? t('customerAccounts.status.enabled')
                        : t('customerAccounts.status.disabled')}
                    </Badge>
                  </div>
                </div>
              </div>
            </div>

            <TextField
              id={`${formId}-email`}
              label={t('customerAccounts.detail.email')}
              error={fieldErrors.email}
              type="email"
              value={form.email}
              onChange={(value) => updateField('email', value)}
              placeholder={t('adminAccounts.form.emailPlaceholder')}
              maxLength={50}
              autoComplete="off"
              autoFocus
            />
            <TextField
              id={`${formId}-full-name`}
              label={t('customerAccounts.detail.fullName')}
              error={fieldErrors.fullName}
              value={form.fullName}
              onChange={(value) => updateField('fullName', value)}
              placeholder={t('adminAccounts.form.fullNamePlaceholder')}
              maxLength={50}
            />
            <TextField
              id={`${formId}-phone-number`}
              label={t('customerAccounts.detail.phoneNumber')}
              error={fieldErrors.phoneNumber}
              type="tel"
              value={form.phoneNumber}
              onChange={(value) => updateField('phoneNumber', value)}
              placeholder={t('adminAccounts.form.phoneNumberPlaceholder')}
              maxLength={15}
            />

            <div className="flex justify-end gap-2 border-t border-border pt-4 sm:col-span-2">
              <Button variant="outline" type="button" onClick={onCancel} disabled={isSubmitting}>
                {t('common.cancel')}
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? submittingLabel : submitLabel}
              </Button>
            </div>
          </FieldGroup>
        </form>
      </CardContent>
    </Card>
  )
}

function TextField({ id, label, error, onChange, disabled = false, ...inputProps }) {
  return (
    <Field invalid={Boolean(error)} disabled={disabled}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Input
        id={id}
        onChange={(event) => onChange(event.target.value)}
        disabled={disabled}
        aria-invalid={Boolean(error)}
        {...inputProps}
      />
      {error ? <FieldError>{error}</FieldError> : null}
    </Field>
  )
}

function validateForm(form, t) {
  const errors = {}
  const email = form.email.trim()

  if (!email) errors.email = t('adminAccounts.validation.emailRequired')
  else if (email.length > 50) errors.email = t('adminAccounts.validation.emailTooLong')
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) errors.email = t('adminAccounts.validation.emailInvalid')

  if (form.fullName.trim().length > 50) errors.fullName = t('adminAccounts.validation.fullNameTooLong')
  if (form.phoneNumber.trim().length > 15) errors.phoneNumber = t('adminAccounts.validation.phoneNumberTooLong')

  return errors
}

export { CustomerAccountForm }
