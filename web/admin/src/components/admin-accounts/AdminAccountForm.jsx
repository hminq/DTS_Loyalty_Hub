import { EyeIcon, EyeSlashIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { toFieldErrorMap } from '../../api/apiError'
import { Button } from '../ui/button'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'

const emptyForm = Object.freeze({
  username: '',
  email: '',
  fullName: '',
  phoneNumber: '',
  roleId: '',
  password: '',
  confirmPassword: '',
})

function AdminAccountForm({ onSubmit, onCancel, roleOptions, isLoadingRoles, roleOptionsError, t }) {
  const [form, setForm] = useState(emptyForm)
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
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
        username: form.username.trim(),
        email: form.email.trim(),
        password: form.password,
        fullName: form.fullName.trim() || null,
        phoneNumber: form.phoneNumber.trim() || null,
        roleId: form.roleId,
      })
    } catch (error) {
      setFieldErrors(toFieldErrorMap(error.details))
      setFormError(error.message || t('adminAccounts.createError'))
      setIsSubmitting(false)
    }
  }

  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <CardContent className="p-5">
        <form className="grid gap-4 sm:grid-cols-2" onSubmit={handleSubmit} noValidate>
          {formError ? (
            <p className="sm:col-span-2 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive">
              {formError}
            </p>
          ) : null}

          <FormField label={t('adminAccounts.form.username')} error={fieldErrors.username}>
            <Input value={form.username} onChange={(event) => updateField('username', event.target.value)} placeholder={t('adminAccounts.form.usernamePlaceholder')} maxLength={50} autoComplete="off" autoFocus aria-invalid={Boolean(fieldErrors.username)} />
          </FormField>
          <FormField label={t('adminAccounts.form.email')} error={fieldErrors.email}>
            <Input type="email" value={form.email} onChange={(event) => updateField('email', event.target.value)} placeholder={t('adminAccounts.form.emailPlaceholder')} maxLength={50} autoComplete="off" aria-invalid={Boolean(fieldErrors.email)} />
          </FormField>
          <FormField label={t('adminAccounts.form.fullName')} error={fieldErrors.fullName}>
            <Input value={form.fullName} onChange={(event) => updateField('fullName', event.target.value)} placeholder={t('adminAccounts.form.fullNamePlaceholder')} maxLength={50} aria-invalid={Boolean(fieldErrors.fullName)} />
          </FormField>
          <FormField label={t('adminAccounts.form.phoneNumber')} error={fieldErrors.phoneNumber}>
            <Input type="tel" value={form.phoneNumber} onChange={(event) => updateField('phoneNumber', event.target.value)} placeholder={t('adminAccounts.form.phoneNumberPlaceholder')} maxLength={15} aria-invalid={Boolean(fieldErrors.phoneNumber)} />
          </FormField>

          <label className="flex flex-col gap-1.5 text-xs font-medium sm:col-span-2">
            {t('adminAccounts.form.role')}
            <select
              className="h-9 w-full rounded-md border border-input bg-background px-3 text-[13px] shadow-xs outline-none focus:border-ring focus:ring-2 focus:ring-ring/20 disabled:cursor-not-allowed disabled:opacity-50"
              value={form.roleId}
              onChange={(event) => updateField('roleId', event.target.value)}
              disabled={isLoadingRoles}
              aria-invalid={Boolean(roleOptionsError || fieldErrors.roleId)}
            >
              <option value="">{isLoadingRoles ? t('adminAccounts.filters.loadingRoles') : t('adminAccounts.filters.chooseRole')}</option>
              {roleOptions.map((role) => <option key={role.roleId} value={role.roleId}>{role.name}</option>)}
            </select>
            {roleOptionsError || fieldErrors.roleId ? <span className="font-normal text-destructive">{roleOptionsError || fieldErrors.roleId}</span> : null}
          </label>

          <FormField label={t('adminAccounts.form.password')} error={fieldErrors.password}>
            <PasswordInput value={form.password} onChange={(value) => updateField('password', value)} placeholder={t('adminAccounts.form.passwordPlaceholder')} visible={showPassword} onToggle={() => setShowPassword((current) => !current)} />
          </FormField>
          <FormField label={t('adminAccounts.form.confirmPassword')} error={fieldErrors.confirmPassword}>
            <PasswordInput value={form.confirmPassword} onChange={(value) => updateField('confirmPassword', value)} placeholder={t('adminAccounts.form.confirmPasswordPlaceholder')} visible={showPassword} onToggle={() => setShowPassword((current) => !current)} />
          </FormField>

          <div className="flex justify-end gap-2 border-t border-border pt-4 sm:col-span-2">
            <Button variant="outline" onClick={onCancel} disabled={isSubmitting}>{t('adminAccounts.cancel')}</Button>
            <Button type="submit" disabled={isSubmitting || isLoadingRoles || Boolean(roleOptionsError)}>
              {isSubmitting ? t('adminAccounts.submitting') : t('adminAccounts.submit')}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

function FormField({ label, error, children }) {
  return (
    <label className="flex flex-col gap-1.5 text-xs font-medium">
      {label}
      {children}
      {error ? <span className="font-normal text-destructive">{error}</span> : null}
    </label>
  )
}

function PasswordInput({ value, onChange, placeholder, visible, onToggle }) {
  const Icon = visible ? EyeSlashIcon : EyeIcon

  return (
    <div className="relative">
      <Input className="pr-10" type={visible ? 'text' : 'password'} value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} maxLength={100} autoComplete="new-password" />
      <button type="button" className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground" onClick={onToggle} tabIndex={-1}>
        <Icon size={17} />
      </button>
    </div>
  )
}

function validateForm(form, t) {
  const errors = {}
  const username = form.username.trim()
  const email = form.email.trim()

  if (!username) errors.username = t('adminAccounts.validation.usernameRequired')
  else if (username.length > 50) errors.username = t('adminAccounts.validation.usernameTooLong')
  if (!email) errors.email = t('adminAccounts.validation.emailRequired')
  else if (email.length > 50) errors.email = t('adminAccounts.validation.emailTooLong')
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) errors.email = t('adminAccounts.validation.emailInvalid')
  if (form.fullName.trim().length > 50) errors.fullName = t('adminAccounts.validation.fullNameTooLong')
  if (form.phoneNumber.trim().length > 15) errors.phoneNumber = t('adminAccounts.validation.phoneNumberTooLong')
  if (!form.roleId) errors.roleId = t('adminAccounts.validation.roleRequired')
  if (!form.password) errors.password = t('adminAccounts.validation.passwordRequired')
  else if (form.password.length < 8) errors.password = t('adminAccounts.validation.passwordTooShort')
  else if (form.password.length > 100) errors.password = t('adminAccounts.validation.passwordTooLong')
  if (!form.confirmPassword) errors.confirmPassword = t('adminAccounts.validation.confirmPasswordRequired')
  else if (form.password !== form.confirmPassword) errors.confirmPassword = t('adminAccounts.validation.passwordsMismatch')

  return errors
}

export { AdminAccountForm }
