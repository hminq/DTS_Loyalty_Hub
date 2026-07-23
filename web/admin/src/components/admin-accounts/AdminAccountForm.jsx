import { EyeIcon, EyeSlashIcon } from '@phosphor-icons/react'
import { useId, useState } from 'react'

import { toFieldErrorMap } from '../../api/apiError'
import { Button } from '../ui/button'
import { Card, CardContent } from '../ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { RoleSearchSelect } from '../roles/RoleSearchSelect'

const emptyForm = Object.freeze({
  username: '',
  email: '',
  fullName: '',
  phoneNumber: '',
  roleId: '',
  roleName: '',
  password: '',
  confirmPassword: '',
})

function AdminAccountForm({
  mode = 'create',
  initialValues = emptyForm,
  submitLabel,
  submittingLabel,
  submitError,
  onSubmit,
  onCancel,
  t,
}) {
  const isCreate = mode === 'create'
  const formId = useId()
  const [form, setForm] = useState(() => ({ ...emptyForm, ...initialValues }))
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

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
    const validationErrors = validateForm(form, isCreate, t)

    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors)
      return
    }

    setIsSubmitting(true)
    setFieldErrors({})
    setFormError('')

    try {
      await onSubmit({
        ...(isCreate ? {
          username: form.username.trim(),
          password: form.password,
        } : {}),
        email: form.email.trim(),
        fullName: form.fullName.trim() || null,
        phoneNumber: form.phoneNumber.trim() || null,
        roleId: form.roleId,
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

            <TextField
              id={`${formId}-username`}
              label={t('adminAccounts.form.username')}
              error={fieldErrors.username}
              value={form.username}
              onChange={(value) => updateField('username', value)}
              placeholder={t('adminAccounts.form.usernamePlaceholder')}
              maxLength={50}
              autoComplete="off"
              autoFocus={isCreate}
              disabled={!isCreate}
            />
            <TextField
              id={`${formId}-email`}
              label={t('adminAccounts.form.email')}
              error={fieldErrors.email}
              type="email"
              value={form.email}
              onChange={(value) => updateField('email', value)}
              placeholder={t('adminAccounts.form.emailPlaceholder')}
              maxLength={50}
              autoComplete="off"
              autoFocus={!isCreate}
            />
            <TextField
              id={`${formId}-full-name`}
              label={t('adminAccounts.form.fullName')}
              error={fieldErrors.fullName}
              value={form.fullName}
              onChange={(value) => updateField('fullName', value)}
              placeholder={t('adminAccounts.form.fullNamePlaceholder')}
              maxLength={50}
            />
            <TextField
              id={`${formId}-phone-number`}
              label={t('adminAccounts.form.phoneNumber')}
              error={fieldErrors.phoneNumber}
              type="tel"
              value={form.phoneNumber}
              onChange={(value) => updateField('phoneNumber', value)}
              placeholder={t('adminAccounts.form.phoneNumberPlaceholder')}
              maxLength={15}
            />

            <Field className="sm:col-span-2" invalid={Boolean(fieldErrors.roleId)}>
              <FieldLabel>{t('adminAccounts.form.role')}</FieldLabel>
              <RoleSearchSelect
                value={form.roleId}
                selectedLabel={form.roleName || undefined}
                onChange={(roleId, role) => {
                  setForm((current) => ({
                    ...current,
                    roleId,
                    roleName: role?.name ?? '',
                  }))
                  clearFieldError('roleId')
                }}
                placeholder={t('adminAccounts.filters.chooseRole')}
                invalid={Boolean(fieldErrors.roleId)}
                ariaLabel={t('adminAccounts.form.role')}
              />
              {fieldErrors.roleId ? <FieldError>{fieldErrors.roleId}</FieldError> : null}
            </Field>

            {isCreate ? (
              <>
                <PasswordField
                  id={`${formId}-password`}
                  label={t('adminAccounts.form.password')}
                  error={fieldErrors.password}
                  value={form.password}
                  onChange={(value) => updateField('password', value)}
                  placeholder={t('adminAccounts.form.passwordPlaceholder')}
                  visible={showPassword}
                  onToggle={() => setShowPassword((current) => !current)}
                  showLabel={t('adminAccounts.form.showPassword')}
                  hideLabel={t('adminAccounts.form.hidePassword')}
                />
                <PasswordField
                  id={`${formId}-confirm-password`}
                  label={t('adminAccounts.form.confirmPassword')}
                  error={fieldErrors.confirmPassword}
                  value={form.confirmPassword}
                  onChange={(value) => updateField('confirmPassword', value)}
                  placeholder={t('adminAccounts.form.confirmPasswordPlaceholder')}
                  visible={showPassword}
                  onToggle={() => setShowPassword((current) => !current)}
                  showLabel={t('adminAccounts.form.showPassword')}
                  hideLabel={t('adminAccounts.form.hidePassword')}
                />
              </>
            ) : null}

            <div className="flex justify-end gap-2 border-t border-border pt-4 sm:col-span-2">
              <Button variant="outline" onClick={onCancel} disabled={isSubmitting}>{t('adminAccounts.cancel')}</Button>
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

function PasswordField({
  id,
  label,
  error,
  value,
  onChange,
  placeholder,
  visible,
  onToggle,
  showLabel,
  hideLabel,
}) {
  const Icon = visible ? EyeSlashIcon : EyeIcon

  return (
    <Field invalid={Boolean(error)}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <div className="relative">
        <Input
          id={id}
          className="pr-10"
          type={visible ? 'text' : 'password'}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={placeholder}
          maxLength={100}
          autoComplete="new-password"
          aria-invalid={Boolean(error)}
        />
        <button
          type="button"
          className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
          onClick={onToggle}
          aria-label={visible ? hideLabel : showLabel}
        >
          <Icon aria-hidden="true" />
        </button>
      </div>
      {error ? <FieldError>{error}</FieldError> : null}
    </Field>
  )
}

function validateForm(form, isCreate, t) {
  const errors = {}
  const username = form.username.trim()
  const email = form.email.trim()

  if (isCreate && !username) errors.username = t('adminAccounts.validation.usernameRequired')
  else if (isCreate && username.length > 50) errors.username = t('adminAccounts.validation.usernameTooLong')
  if (!email) errors.email = t('adminAccounts.validation.emailRequired')
  else if (email.length > 50) errors.email = t('adminAccounts.validation.emailTooLong')
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) errors.email = t('adminAccounts.validation.emailInvalid')
  if (form.fullName.trim().length > 50) errors.fullName = t('adminAccounts.validation.fullNameTooLong')
  if (form.phoneNumber.trim().length > 15) errors.phoneNumber = t('adminAccounts.validation.phoneNumberTooLong')
  if (!form.roleId) errors.roleId = t('adminAccounts.validation.roleRequired')

  if (isCreate) {
    if (!form.password) errors.password = t('adminAccounts.validation.passwordRequired')
    else if (form.password.length < 8) errors.password = t('adminAccounts.validation.passwordTooShort')
    else if (form.password.length > 100) errors.password = t('adminAccounts.validation.passwordTooLong')
    if (!form.confirmPassword) errors.confirmPassword = t('adminAccounts.validation.confirmPasswordRequired')
    else if (form.password !== form.confirmPassword) errors.confirmPassword = t('adminAccounts.validation.passwordsMismatch')
  }

  return errors
}

export { AdminAccountForm }
