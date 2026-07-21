import { useState } from 'react'
import { useTranslation } from 'react-i18next'

import { toFieldErrorMap } from '../../api/apiError'
import { PermissionMatrix } from '../permissions/PermissionMatrix'
import { Button } from '../ui/button'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'

function RoleForm({ initialValues, permissionGroups, submitLabel, submittingLabel, onSubmit, onCancel }) {
  const { t } = useTranslation()
  const [name, setName] = useState(initialValues.name ?? '')
  const [permissionIds, setPermissionIds] = useState(initialValues.permissionIds ?? [])
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function togglePermission(permissionId) {
    setPermissionIds((current) => current.includes(permissionId)
      ? current.filter((id) => id !== permissionId)
      : [...current, permissionId])
    clearFieldError('permissionIds')
  }

  function togglePermissionGroup(groupPermissionIds) {
    setPermissionIds((current) => {
      const currentIds = new Set(current)
      const allSelected = groupPermissionIds.every((permissionId) => currentIds.has(permissionId))

      if (allSelected) {
        groupPermissionIds.forEach((permissionId) => currentIds.delete(permissionId))
      } else {
        groupPermissionIds.forEach((permissionId) => currentIds.add(permissionId))
      }

      return [...currentIds]
    })
    clearFieldError('permissionIds')
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
    const validationErrors = validateRole(name, permissionIds, t)

    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors)
      return
    }

    setIsSubmitting(true)
    setFieldErrors({})
    setFormError('')

    try {
      await onSubmit({ name: name.trim(), permissionIds })
    } catch (error) {
      setFieldErrors(toFieldErrorMap(error.details))
      setFormError(error.message || t('roles.form.submitError'))
      setIsSubmitting(false)
    }
  }

  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <CardContent className="p-5">
        <form className="flex flex-col gap-5" onSubmit={handleSubmit} noValidate>
          {formError ? (
            <p className="rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive">
              {formError}
            </p>
          ) : null}

          <label className="flex max-w-xl flex-col gap-1.5 text-xs font-medium">
            {t('roles.form.name')}
            <Input
              value={name}
              onChange={(event) => {
                setName(event.target.value)
                clearFieldError('name')
              }}
              placeholder={t('roles.form.namePlaceholder')}
              maxLength={100}
              autoFocus
              aria-invalid={Boolean(fieldErrors.name)}
            />
            {fieldErrors.name ? <span className="font-normal text-destructive">{fieldErrors.name}</span> : null}
          </label>

          <div className="flex flex-col gap-2">
            <div>
              <h2 className="text-xs font-semibold">{t('roles.form.permissions')}</h2>
              <p className="mt-1 text-[12px] text-muted-foreground">{t('roles.form.permissionsDescription')}</p>
            </div>
            <PermissionMatrix
              groups={permissionGroups}
              selectedPermissionIds={permissionIds}
              onPermissionToggle={togglePermission}
              onPermissionGroupToggle={togglePermissionGroup}
              labels={{
                group: t('permissions.matrix.group'),
                all: t('permissions.matrix.all'),
                selectAll: t('permissions.matrix.selectAll'),
                selectGroup: (groupName) => t('permissions.matrix.selectGroup', { groupName }),
                defined: t('permissions.matrix.defined'),
                notDefined: t('permissions.matrix.notDefined'),
              }}
            />
            {fieldErrors.permissionIds ? <span className="text-xs text-destructive">{fieldErrors.permissionIds}</span> : null}
          </div>

          <div className="flex justify-end gap-2 border-t border-border pt-4">
            <Button variant="outline" onClick={onCancel} disabled={isSubmitting}>{t('common.cancel')}</Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? submittingLabel : submitLabel}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

function validateRole(name, permissionIds, t) {
  const errors = {}
  const normalizedName = name.trim()

  if (!normalizedName) errors.name = t('roles.validation.nameRequired')
  else if (normalizedName.length > 100) errors.name = t('roles.validation.nameTooLong')
  if (permissionIds.length === 0) errors.permissionIds = t('roles.validation.permissionRequired')

  return errors
}

export { RoleForm }
