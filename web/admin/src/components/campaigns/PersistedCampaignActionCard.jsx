import { GiftIcon, PencilSimpleIcon, TrashIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Combobox } from '../ui/combobox'
import { Field, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { formatCampaignNumber } from './campaignFormatters'
import { buildCampaignActionPayload, mapCampaignActionToFormValues } from './campaignPayloads'
import { validateCampaignAction } from './campaignValidation'

export function PersistedCampaignActionCard({
  action,
  isNew = false,
  isDraft = true,
  canEdit = true,
  options = {},
  eventType = '',
  isReferralOnly = false,
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

  function handleActionTypeChange(nextType) {
    const selectedAction = (options.actionTypes || []).find((a) => a.value === nextType)
    const compatibleCalcs = (selectedAction?.calculationTypes || []).map((c) => c.value)
    const compatibleRecipients = (selectedAction?.recipients || []).map((r) => r.value)

    setFormValues((prev) => ({
      ...prev,
      actionType: nextType,
      calculationType: compatibleCalcs.includes(prev.calculationType) ? prev.calculationType : '',
      recipient: compatibleRecipients.includes(prev.recipient) ? prev.recipient : '',
    }))
  }

  const selectedEvent = (options.eventTypes || []).find((e) => e.value === eventType)
  const compatibleActionCodes = selectedEvent?.actionTypes || []
  const actionTypeOptions = (options.actionTypes || []).filter(
    (a) => !eventType || compatibleActionCodes.includes(a.value),
  )

  const selectedAction = (options.actionTypes || []).find((a) => a.value === formValues.actionType)
  const calculationTypeOptions = selectedAction?.calculationTypes || []

  const allRecipients = selectedAction?.recipients || []
  const recipientOptions = allRecipients.map((rec) => {
    if (rec.value === 'REFERRER' && !isReferralOnly) {
      return { ...rec, disabled: true }
    }
    return rec
  })

  async function handleSave(e) {
    e.preventDefault()
    setCardError('')
    setFieldErrors({})

    const validation = validateCampaignAction(formValues, t)
    if (!validation.isValid) {
      setFieldErrors(validation.errors)
      return
    }

    const payload = buildCampaignActionPayload(
      formValues,
      Number(formValues.executeOrder || 1),
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
    const config = action.actionConfig || {}
    const actionTypeLabel = t(`campaigns.actionTypes.${action.actionType}`, {
      defaultValue: action.actionType || '—',
    })
    const calculationTypeLabel = t(
      `campaigns.calculationTypes.${config.calculationType}`,
      {
        defaultValue: config.calculationType || '—',
      },
    )
    const recipientLabel = t(`campaigns.recipients.${config.recipient}`, {
      defaultValue: config.recipient || '—',
    })

    return (
      <Card className="shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <div className="flex items-center gap-2">
            <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
              {action.executeOrder ?? 1}
            </span>
            <CardTitle className="text-sm font-semibold">{actionTypeLabel}</CardTitle>
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
          <div className="grid gap-4 sm:grid-cols-4">
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.calculationTypeLabel', {
                  defaultValue: 'Calculation type',
                })}
              </p>
              <p className="mt-1 text-sm font-medium text-foreground">
                {calculationTypeLabel}
              </p>
            </div>

            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}
              </p>
              <p className="mt-1 text-sm font-medium text-foreground">{recipientLabel}</p>
            </div>

            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.amountLabel', { defaultValue: 'Reward amount' })}
              </p>
              <p className="mt-1 text-sm font-semibold text-primary">
                {config.amount != null
                  ? formatCampaignNumber(config.amount, language)
                  : '0'}{' '}
                {t('campaigns.detail.points', { defaultValue: 'points' })}
              </p>
            </div>

            <div>
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
            <div className="grid gap-4 sm:grid-cols-3">
              <Field invalid={Boolean(fieldErrors.actionType)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}
                </FieldLabel>
                <Combobox
                  value={formValues.actionType}
                  onValueChange={handleActionTypeChange}
                  options={actionTypeOptions}
                  placeholder={t('campaigns.form.selectActionType', {
                    defaultValue: 'Select action type',
                  })}
                  emptyOptionLabel={t('campaigns.form.selectActionType', {
                    defaultValue: 'Select action type',
                  })}
                  ariaLabel={t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}
                  disabled={isSubmitting}
                />
                {fieldErrors.actionType ? (
                  <FieldError>{fieldErrors.actionType}</FieldError>
                ) : null}
              </Field>

              <Field
                invalid={Boolean(fieldErrors.calculationType)}
                disabled={!formValues.actionType || isSubmitting}
              >
                <FieldLabel>
                  {t('campaigns.form.calculationTypeLabel', {
                    defaultValue: 'Calculation type',
                  })}
                </FieldLabel>
                <Combobox
                  value={formValues.calculationType}
                  onValueChange={(val) => updateField('calculationType', val)}
                  options={calculationTypeOptions}
                  placeholder={t('campaigns.form.selectCalculationType', {
                    defaultValue: 'Select calculation type',
                  })}
                  emptyOptionLabel={t('campaigns.form.selectCalculationType', {
                    defaultValue: 'Select calculation type',
                  })}
                  disabled={!formValues.actionType || isSubmitting}
                  ariaLabel={t('campaigns.form.calculationTypeLabel', {
                    defaultValue: 'Calculation type',
                  })}
                />
                {fieldErrors.calculationType ? (
                  <FieldError>{fieldErrors.calculationType}</FieldError>
                ) : null}
              </Field>

              <Field
                invalid={Boolean(fieldErrors.recipient)}
                disabled={!formValues.actionType || isSubmitting}
              >
                <FieldLabel>
                  {t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}
                </FieldLabel>
                <Combobox
                  value={formValues.recipient}
                  onValueChange={(val) => updateField('recipient', val)}
                  options={recipientOptions}
                  placeholder={t('campaigns.form.selectRecipient', {
                    defaultValue: 'Select recipient',
                  })}
                  emptyOptionLabel={t('campaigns.form.selectRecipient', {
                    defaultValue: 'Select recipient',
                  })}
                  disabled={!formValues.actionType || isSubmitting}
                  ariaLabel={t('campaigns.form.recipientLabel', {
                    defaultValue: 'Recipient',
                  })}
                />
                {fieldErrors.recipient ? (
                  <FieldError>{fieldErrors.recipient}</FieldError>
                ) : null}
              </Field>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field invalid={Boolean(fieldErrors.amount)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.amountLabel', { defaultValue: 'Reward amount (points)' })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0.01"
                  step="any"
                  value={formValues.amount}
                  onChange={(e) => updateField('amount', e.target.value)}
                  placeholder="50"
                  aria-invalid={Boolean(fieldErrors.amount)}
                  disabled={isSubmitting}
                />
                {fieldErrors.amount ? <FieldError>{fieldErrors.amount}</FieldError> : null}
              </Field>

              <Field invalid={Boolean(fieldErrors.executeOrder)} disabled={isSubmitting}>
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
                  aria-invalid={Boolean(fieldErrors.executeOrder)}
                  disabled={isSubmitting}
                />
                {fieldErrors.executeOrder ? (
                  <FieldError>{fieldErrors.executeOrder}</FieldError>
                ) : null}
              </Field>
            </div>
          </FieldGroup>

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
