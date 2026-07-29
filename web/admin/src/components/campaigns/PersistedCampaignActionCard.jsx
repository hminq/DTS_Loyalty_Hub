import { GiftIcon, PencilSimpleIcon, TrashIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { formatCampaignNumber } from './campaignFormatters'
import { buildCampaignActionPayload, mapCampaignActionToFormValues } from './campaignPayloads'
import { validateCampaignAction } from './campaignValidation'
import { describeCampaignAction } from './campaignPresentation'
import { CampaignActionConfigurationFields } from './CampaignActionConfigurationFields'

export function PersistedCampaignActionCard({
  action,
  isNew = false,
  isDraft = true,
  canEdit = true,
  options = {},
  eventType = '',
  conditionPresetCode = '',
  isSubmitting = false,
  externalError = '',
  externalFieldErrors = {},
  onSave,
  onCancelNew,
  onDelete,
  language,
  t,
}) {
  const [isEditing, setIsEditing] = useState(isNew)
  const [formValues, setFormValues] = useState(() => mapCampaignActionToFormValues(action || {}))
  const [fieldErrors, setFieldErrors] = useState({})
  const [cardError, setCardError] = useState(externalError || '')

  const allErrors = { ...externalFieldErrors, ...fieldErrors }

  function updateField(field, value) {
    setFormValues((prev) => ({ ...prev, [field]: value }))
    if (fieldErrors[field]) {
      setFieldErrors((prev) => {
        const next = { ...prev }
        delete next[field]
        return next
      })
    }
  }

  function handleConfigChange(nextActionValues) {
    setFormValues((prev) => ({
      ...prev,
      ...nextActionValues,
    }))
  }

  async function handleSave(e) {
    e.preventDefault()
    setCardError('')
    setFieldErrors({})

    const validation = validateCampaignAction(
      formValues,
      options,
      { eventType, conditionPresetCode },
      t,
    )
    if (!validation.isValid) {
      setFieldErrors(validation.errors)
      return
    }

    const payload = buildCampaignActionPayload(
      formValues,
      Number(formValues.executeOrder || 1),
      options
    )

    try {
      if (onSave) {
        await onSave(action?.actionId || null, payload, formValues)
      }
      if (!isNew) {
        setIsEditing(false)
      }
    } catch (err) {
      if (!err.details || err.details.length === 0) {
        setCardError(
          err.message ||
            t('campaigns.actions.saveFailed', { defaultValue: 'Failed to save action.' }),
        )
      }
    }
  }

  function handleCancel() {
    setCardError('')
    setFieldErrors({})
    if (isNew) {
      if (onCancelNew) onCancelNew()
    } else {
      setFormValues(mapCampaignActionToFormValues(action || {}))
      setIsEditing(false)
    }
  }

  if (!isEditing && action) {
    const actionDesc = describeCampaignAction({ action, eventType, options, t })

    return (
      <Card className="shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <div className="flex items-center gap-2">
            <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
              {actionDesc.executeOrder}
            </span>
            <CardTitle className="text-sm font-semibold">{actionDesc.actionTypeLabel}</CardTitle>
          </div>
          {isDraft && canEdit ? (
            <div className="flex items-center gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="h-7 gap-1 px-2 text-xs"
                onClick={() => setIsEditing(true)}
              >
                <PencilSimpleIcon size={14} weight="bold" />
                {t('common.edit', { defaultValue: 'Edit' })}
              </Button>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="h-7 gap-1 px-2 text-xs text-muted-foreground hover:text-destructive"
                onClick={() => onDelete && onDelete(action)}
              >
                <TrashIcon size={14} weight="bold" />
                {t('common.delete', { defaultValue: 'Delete' })}
              </Button>
            </div>
          ) : null}
        </CardHeader>
        <CardContent>
          {!actionDesc.isSupported && (
            <div className="mb-4 rounded-md bg-amber-50 px-3 py-2 text-xs font-medium text-amber-700">
              {t('campaigns.detail.unsupportedAction', { defaultValue: 'Warning: Unsupported action configuration.' })}
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-4">
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.targetSelectorLabel', { defaultValue: 'Target' })}
              </p>
              <p className="mt-1 text-sm font-medium text-foreground">
                {actionDesc.targetLabel}
              </p>
            </div>

            {actionDesc.parameters.map(param => (
              <div key={param.code}>
                <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                  {param.label}
                </p>
                <p className={`mt-1 text-sm font-semibold ${param.isKnown ? 'text-primary' : 'text-amber-600'}`}>
                  {param.dataType === 'DECIMAL' && typeof param.value === 'number'
                    ? formatCampaignNumber(param.value, language)
                    : String(param.value)}
                </p>
              </div>
            ))}

            <div className={actionDesc.parameters.length === 0 ? 'sm:col-span-2' : ''}>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.actionLimitsTitle', {
                  defaultValue: 'Action execution limits',
                })}
              </p>
              <div className="mt-1 space-y-0.5 text-xs text-muted-foreground">
                <div>
                  <span className="font-medium">
                    {t('campaigns.detail.actionTotalLimit', {
                      defaultValue: 'Total action execution limit',
                    })}:
                  </span>{' '}
                  {action.totalCount != null
                    ? formatCampaignNumber(action.totalCount, language)
                    : t('campaigns.detail.unlimited', { defaultValue: 'Unlimited' })}
                </div>
                <div>
                  <span className="font-medium">
                    {t('campaigns.detail.actionSessionLimit', {
                      defaultValue: 'Action execution limit per session',
                    })}:
                  </span>{' '}
                  {action.sessionCount != null
                    ? formatCampaignNumber(action.sessionCount, language)
                    : t('campaigns.detail.unlimited', { defaultValue: 'Unlimited' })}
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className="border-primary/30 shadow-md">
      <CardHeader>
        <CardTitle className="text-sm font-semibold">
          {isNew
            ? t('campaigns.actions.addNewTitle', { defaultValue: 'Add reward action' })
            : t('campaigns.actions.editTitle', {
                defaultValue: 'Edit reward action',
              })}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSave} className="grid gap-4">
          {cardError || externalError ? (
            <div className="rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
              {cardError || externalError}
            </div>
          ) : null}

          <FieldGroup>
            <CampaignActionConfigurationFields
              action={formValues}
              options={options}
              eventType={eventType}
              conditionPresetCode={conditionPresetCode}
              fieldErrors={allErrors}
              isSubmitting={isSubmitting}
              onChange={handleConfigChange}
              t={t}
            />

            <div className="grid gap-4 sm:grid-cols-2 mt-4 border-t border-border pt-4">
              <Field invalid={Boolean(allErrors.executeOrder)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.actions.executeOrderLabel', {
                    defaultValue: 'Execution order',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="1"
                  step="1"
                  value={formValues.executeOrder}
                  onChange={(e) => updateField('executeOrder', e.target.value)}
                  placeholder="1"
                  aria-invalid={Boolean(allErrors.executeOrder)}
                  disabled={isSubmitting}
                />
                {allErrors.executeOrder ? (
                  <FieldError>{allErrors.executeOrder}</FieldError>
                ) : null}
              </Field>
            </div>
          </FieldGroup>

          <div className="border-t border-border pt-4">
            <h4 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.actionLimitsTitle', {
                defaultValue: 'Action execution limits',
              })}
            </h4>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field
                invalid={Boolean(allErrors.totalCount)}
                disabled={isSubmitting}
              >
                <FieldLabel>
                  {t('campaigns.form.actionLimitTotalLabel', {
                    defaultValue: 'Total action execution limit',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0"
                  step="1"
                  inputMode="numeric"
                  value={formValues.totalCount ?? ''}
                  onChange={(e) => updateField('totalCount', e.target.value)}
                  placeholder={t('campaigns.detail.unlimited', {
                    defaultValue: 'Unlimited',
                  })}
                  aria-invalid={Boolean(allErrors.totalCount)}
                  disabled={isSubmitting}
                />
                {allErrors.totalCount ? (
                  <FieldError>{allErrors.totalCount}</FieldError>
                ) : (
                  <p className="text-[11px] text-muted-foreground">
                    {t('campaigns.form.actionLimitTotalHelper', {
                      defaultValue:
                        'Maximum successful executions of this action across the campaign. Leave empty for unlimited.',
                    })}
                  </p>
                )}
              </Field>

              <Field
                invalid={Boolean(allErrors.sessionCount)}
                disabled={isSubmitting}
              >
                <FieldLabel>
                  {t('campaigns.form.actionLimitSessionLabel', {
                    defaultValue: 'Action execution limit per session',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0"
                  step="1"
                  inputMode="numeric"
                  value={formValues.sessionCount ?? ''}
                  onChange={(e) => updateField('sessionCount', e.target.value)}
                  placeholder={t('campaigns.detail.unlimited', {
                    defaultValue: 'Unlimited',
                  })}
                  aria-invalid={Boolean(allErrors.sessionCount)}
                  disabled={isSubmitting}
                />
                {allErrors.sessionCount ? (
                  <FieldError>{allErrors.sessionCount}</FieldError>
                ) : (
                  <p className="text-[11px] text-muted-foreground">
                    {t('campaigns.form.actionLimitSessionHelper', {
                      defaultValue:
                        'Maximum successful executions of this action in each session. Leave empty for unlimited.',
                    })}
                  </p>
                )}
              </Field>
            </div>
          </div>

          <div className="flex items-center justify-end gap-2 border-t border-border pt-4">
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handleCancel}
              disabled={isSubmitting}
            >
              {t('common.cancel', { defaultValue: 'Cancel' })}
            </Button>
            <Button type="submit" size="sm" disabled={isSubmitting}>
              {isSubmitting
                ? t('campaigns.actions.saving', { defaultValue: 'Saving...' })
                : t('campaigns.actions.save', { defaultValue: 'Save action' })}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}
