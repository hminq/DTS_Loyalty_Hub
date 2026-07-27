import { TrashIcon } from '@phosphor-icons/react'

import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Combobox } from '../ui/combobox'
import { Field, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'

export function CampaignActionCard({
  index,
  action,
  options = {},
  eventType = '',
  isReferralOnly = false,
  fieldErrors = {},
  cardError = '',
  isSubmitting = false,
  canRemove = false,
  onChange,
  onRemove,
  t,
}) {
  const prefix = `actions[${index}]`

  function updateField(field, value) {
    onChange(index, { ...action, [field]: value })
  }

  function handleActionTypeChange(nextActionType) {
    const selectedAction = (options.actionTypes || []).find((a) => a.value === nextActionType)
    const compatibleCalculations = (selectedAction?.calculationTypes || []).map((c) => c.value)
    const compatibleRecipients = (selectedAction?.recipients || []).map((r) => r.value)

    onChange(index, {
      ...action,
      actionType: nextActionType,
      calculationType: compatibleCalculations.includes(action.calculationType)
        ? action.calculationType
        : '',
      recipient: compatibleRecipients.includes(action.recipient) ? action.recipient : '',
    })
  }

  const selectedEvent = (options.eventTypes || []).find((e) => e.value === eventType)
  const compatibleActionCodes = selectedEvent?.actionTypes || []

  const actionTypeOptions = (options.actionTypes || []).filter(
    (a) => !eventType || compatibleActionCodes.includes(a.value),
  )

  const selectedAction = (options.actionTypes || []).find((a) => a.value === action.actionType)
  const calculationTypeOptions = selectedAction?.calculationTypes || []

  const allRecipients = selectedAction?.recipients || []
  const recipientOptions = allRecipients.map((rec) => {
    if (rec.value === 'REFERRER' && !isReferralOnly) {
      return { ...rec, disabled: true }
    }
    return rec
  })

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm font-semibold">
          {t('campaigns.form.actionLabel', {
            index: index + 1,
            defaultValue: `Action ${index + 1}`,
          })}
        </CardTitle>
        {canRemove ? (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="h-7 gap-1 px-2 text-xs text-muted-foreground hover:text-destructive"
            onClick={() => onRemove(index)}
            disabled={isSubmitting}
          >
            <TrashIcon size={14} />
            {t('campaigns.form.removeAction', { defaultValue: 'Remove' })}
          </Button>
        ) : null}
      </CardHeader>
      <CardContent>
        <FieldGroup>
          {cardError ? (
            <div className="rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
              {cardError}
            </div>
          ) : null}

          <div className="grid gap-4 sm:grid-cols-3">
            <Field invalid={Boolean(fieldErrors[`${prefix}.actionType`])} disabled={isSubmitting}>
              <FieldLabel>
                {t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}
              </FieldLabel>
              <Combobox
                value={action.actionType}
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
              {fieldErrors[`${prefix}.actionType`] ? (
                <FieldError>{fieldErrors[`${prefix}.actionType`]}</FieldError>
              ) : null}
            </Field>

            <Field
              invalid={Boolean(fieldErrors[`${prefix}.calculationType`])}
              disabled={!action.actionType || isSubmitting}
            >
              <FieldLabel>
                {t('campaigns.form.calculationTypeLabel', { defaultValue: 'Calculation type' })}
              </FieldLabel>
              <Combobox
                value={action.calculationType}
                onValueChange={(val) => updateField('calculationType', val)}
                options={calculationTypeOptions}
                placeholder={t('campaigns.form.selectCalculationType', {
                  defaultValue: 'Select calculation type',
                })}
                emptyOptionLabel={t('campaigns.form.selectCalculationType', {
                  defaultValue: 'Select calculation type',
                })}
                disabled={!action.actionType || isSubmitting}
                ariaLabel={t('campaigns.form.calculationTypeLabel', {
                  defaultValue: 'Calculation type',
                })}
              />
              {fieldErrors[`${prefix}.calculationType`] ? (
                <FieldError>{fieldErrors[`${prefix}.calculationType`]}</FieldError>
              ) : null}
            </Field>

            <Field
              invalid={Boolean(fieldErrors[`${prefix}.recipient`])}
              disabled={!action.actionType || isSubmitting}
            >
              <FieldLabel>
                {t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}
              </FieldLabel>
              <Combobox
                value={action.recipient}
                onValueChange={(val) => updateField('recipient', val)}
                options={recipientOptions}
                placeholder={t('campaigns.form.selectRecipient', {
                  defaultValue: 'Select recipient',
                })}
                emptyOptionLabel={t('campaigns.form.selectRecipient', {
                  defaultValue: 'Select recipient',
                })}
                disabled={!action.actionType || isSubmitting}
                ariaLabel={t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}
              />
              {fieldErrors[`${prefix}.recipient`] ? (
                <FieldError>{fieldErrors[`${prefix}.recipient`]}</FieldError>
              ) : null}
            </Field>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <Field invalid={Boolean(fieldErrors[`${prefix}.amount`])} disabled={isSubmitting}>
              <FieldLabel>
                {t('campaigns.form.amountLabel', { defaultValue: 'Reward amount' })}
              </FieldLabel>
              <Input
                type="number"
                min="0"
                step="any"
                value={action.amount}
                onChange={(e) => updateField('amount', e.target.value)}
                placeholder="50"
                aria-invalid={Boolean(fieldErrors[`${prefix}.amount`])}
                disabled={isSubmitting}
              />
              {fieldErrors[`${prefix}.amount`] ? (
                <FieldError>{fieldErrors[`${prefix}.amount`]}</FieldError>
              ) : null}
            </Field>

            <Field disabled>
              <FieldLabel>
                {t('campaigns.form.executeOrderLabel', { defaultValue: 'Execution order' })}
              </FieldLabel>
              <Input type="number" value={index + 1} disabled readOnly />
            </Field>
          </div>
        </FieldGroup>
      </CardContent>
    </Card>
  )
}
