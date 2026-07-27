import { InfoIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Combobox } from '../ui/combobox'
import { DateTimePicker } from '../ui/date-time-picker'
import { Field, FieldDescription, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'

export function CampaignForm({
  options = {},
  onCancel,
  t,
}) {
  const [formValues, setFormValues] = useState({
    campaignName: '',
    description: '',
    bannerImageUrl: '',
    eventType: '',
    conditionSource: '',
    startDate: '',
    endDate: '',
    scheduleCron: '',
    durationHour: '',
    userLimitTotal: '',
    userLimitSession: '',
    action: {
      actionType: '',
      calculationType: '',
      recipient: '',
      amount: '',
      executeOrder: 1,
    },
  })

  function updateField(field, value) {
    setFormValues((prev) => ({ ...prev, [field]: value }))
  }

  function updateActionField(field, value) {
    setFormValues((prev) => ({
      ...prev,
      action: {
        ...prev.action,
        [field]: value,
      },
    }))
  }

  function handleEventTypeChange(nextEventType) {
    const selectedEvent = (options.eventTypes || []).find((e) => e.value === nextEventType)
    const compatibleActions = selectedEvent?.actionTypes || []
    const nextActionType = compatibleActions.includes(formValues.action.actionType)
      ? formValues.action.actionType
      : ''

    const selectedAction = (options.actionTypes || []).find((a) => a.value === nextActionType)
    const compatibleCalculations = (selectedAction?.calculationTypes || []).map((c) => c.value)
    const compatibleRecipients = (selectedAction?.recipients || []).map((r) => r.value)

    setFormValues((prev) => ({
      ...prev,
      eventType: nextEventType,
      conditionSource: '',
      action: {
        ...prev.action,
        actionType: nextActionType,
        calculationType: compatibleCalculations.includes(prev.action.calculationType) ? prev.action.calculationType : '',
        recipient: compatibleRecipients.includes(prev.action.recipient) ? prev.action.recipient : '',
      },
    }))
  }

  function handleActionTypeChange(nextActionType) {
    setFormValues((prev) => ({
      ...prev,
      action: {
        ...prev.action,
        actionType: nextActionType,
        calculationType: '',
        recipient: '',
      },
    }))
  }

  const selectedEvent = (options.eventTypes || []).find((e) => e.value === formValues.eventType)
  const conditionSourceOptions = selectedEvent?.sources || []

  const compatibleActionCodes = selectedEvent?.actionTypes || []
  const actionTypeOptions = (options.actionTypes || []).filter(
    (a) => !formValues.eventType || compatibleActionCodes.includes(a.value),
  )

  const selectedAction = (options.actionTypes || []).find((a) => a.value === formValues.action.actionType)
  const calculationTypeOptions = selectedAction?.calculationTypes || []
  const recipientOptions = selectedAction?.recipients || []

  const timeZoneText = t('campaigns.form.timeZoneHelper', {
    timeZone: options.schedule?.timeZone || 'UTC',
    defaultValue: `Schedule operates in ${options.schedule?.timeZone || 'UTC'} timezone.`,
  })

  const weekdayLabels = (options.schedule?.daysOfWeek || []).map((day) => day.label).join(', ')

  function handleSubmit(event) {
    event.preventDefault()
  }

  return (
    <form onSubmit={handleSubmit} className="grid gap-6">
      <Card>
        <CardHeader>
          <CardTitle>{t('campaigns.form.generalTitle', { defaultValue: 'General information' })}</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel>{t('campaigns.form.campaignNameLabel', { defaultValue: 'Campaign name' })}</FieldLabel>
                <Input
                  value={formValues.campaignName}
                  onChange={(e) => updateField('campaignName', e.target.value)}
                  placeholder={t('campaigns.form.campaignNamePlaceholder', { defaultValue: 'Enter campaign name' })}
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.bannerImageUrlLabel', { defaultValue: 'Banner image URL' })}</FieldLabel>
                <Input
                  value={formValues.bannerImageUrl}
                  onChange={(e) => updateField('bannerImageUrl', e.target.value)}
                  placeholder={t('campaigns.form.bannerImageUrlPlaceholder', { defaultValue: 'https://...' })}
                />
              </Field>
            </div>

            <Field>
              <FieldLabel>{t('campaigns.form.descriptionLabel', { defaultValue: 'Description' })}</FieldLabel>
              <Input
                value={formValues.description}
                onChange={(e) => updateField('description', e.target.value)}
                placeholder={t('campaigns.form.descriptionPlaceholder', { defaultValue: 'Describe the campaign purpose and rewards' })}
              />
            </Field>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel>{t('campaigns.form.eventTypeLabel', { defaultValue: 'Event type' })}</FieldLabel>
                <Combobox
                  value={formValues.eventType}
                  onValueChange={handleEventTypeChange}
                  options={options.eventTypes ?? []}
                  placeholder={t('campaigns.form.selectEventType', { defaultValue: 'Select event type' })}
                  emptyOptionLabel={t('campaigns.form.selectEventType', { defaultValue: 'Select event type' })}
                  ariaLabel={t('campaigns.form.eventTypeLabel', { defaultValue: 'Event type' })}
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.conditionSourceLabel', { defaultValue: 'Registration source' })}</FieldLabel>
                <Combobox
                  value={formValues.conditionSource}
                  onValueChange={(val) => updateField('conditionSource', val)}
                  options={conditionSourceOptions}
                  placeholder={t('campaigns.form.selectConditionSource', { defaultValue: 'Select registration source' })}
                  emptyOptionLabel={t('campaigns.form.selectConditionSource', { defaultValue: 'Select registration source' })}
                  disabled={!formValues.eventType}
                  ariaLabel={t('campaigns.form.conditionSourceLabel', { defaultValue: 'Registration source' })}
                />
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('campaigns.form.activeRangeTitle', { defaultValue: 'Active range' })}</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel>{t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}</FieldLabel>
                <DateTimePicker
                  value={formValues.startDate}
                  onChange={(val) => updateField('startDate', val)}
                  ariaLabel={t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}</FieldLabel>
                <DateTimePicker
                  value={formValues.endDate}
                  onChange={(val) => updateField('endDate', val)}
                  ariaLabel={t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
                />
              </Field>
            </div>
            <FieldDescription>{timeZoneText}</FieldDescription>
          </FieldGroup>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('campaigns.form.scheduleTitle', { defaultValue: 'Schedule' })}</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel>{t('campaigns.form.scheduleCronLabel', { defaultValue: 'Schedule CRON' })}</FieldLabel>
                <Input
                  value={formValues.scheduleCron}
                  onChange={(e) => updateField('scheduleCron', e.target.value)}
                  placeholder="0 0 2 * * ?"
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.durationHourLabel', { defaultValue: 'Duration (hours)' })}</FieldLabel>
                <Input
                  type="number"
                  min="1"
                  value={formValues.durationHour}
                  onChange={(e) => updateField('durationHour', e.target.value)}
                  placeholder="2"
                />
              </Field>
            </div>
            {weekdayLabels ? (
              <FieldDescription>
                {t('campaigns.form.daysOfWeekHelper', {
                  days: weekdayLabels,
                  defaultValue: `Available days of week: ${weekdayLabels}`,
                })}
              </FieldDescription>
            ) : null}
          </FieldGroup>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('campaigns.form.limitsTitle', { defaultValue: 'Limits' })}</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel>{t('campaigns.form.userLimitTotalLabel', { defaultValue: 'Total user limit' })}</FieldLabel>
                <Input
                  type="number"
                  min="0"
                  value={formValues.userLimitTotal}
                  onChange={(e) => updateField('userLimitTotal', e.target.value)}
                  placeholder="1000"
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.userLimitSessionLabel', { defaultValue: 'Session user limit' })}</FieldLabel>
                <Input
                  type="number"
                  min="0"
                  value={formValues.userLimitSession}
                  onChange={(e) => updateField('userLimitSession', e.target.value)}
                  placeholder="100"
                />
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('campaigns.form.actionTitle', { defaultValue: 'Default action' })}</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-3">
              <Field>
                <FieldLabel>{t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}</FieldLabel>
                <Combobox
                  value={formValues.action.actionType}
                  onValueChange={handleActionTypeChange}
                  options={actionTypeOptions}
                  placeholder={t('campaigns.form.selectActionType', { defaultValue: 'Select action type' })}
                  emptyOptionLabel={t('campaigns.form.selectActionType', { defaultValue: 'Select action type' })}
                  ariaLabel={t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.calculationTypeLabel', { defaultValue: 'Calculation type' })}</FieldLabel>
                <Combobox
                  value={formValues.action.calculationType}
                  onValueChange={(val) => updateActionField('calculationType', val)}
                  options={calculationTypeOptions}
                  placeholder={t('campaigns.form.selectCalculationType', { defaultValue: 'Select calculation type' })}
                  emptyOptionLabel={t('campaigns.form.selectCalculationType', { defaultValue: 'Select calculation type' })}
                  disabled={!formValues.action.actionType}
                  ariaLabel={t('campaigns.form.calculationTypeLabel', { defaultValue: 'Calculation type' })}
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}</FieldLabel>
                <Combobox
                  value={formValues.action.recipient}
                  onValueChange={(val) => updateActionField('recipient', val)}
                  options={recipientOptions}
                  placeholder={t('campaigns.form.selectRecipient', { defaultValue: 'Select recipient' })}
                  emptyOptionLabel={t('campaigns.form.selectRecipient', { defaultValue: 'Select recipient' })}
                  disabled={!formValues.action.actionType}
                  ariaLabel={t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}
                />
              </Field>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field>
                <FieldLabel>{t('campaigns.form.amountLabel', { defaultValue: 'Reward amount' })}</FieldLabel>
                <Input
                  type="number"
                  min="0"
                  value={formValues.action.amount}
                  onChange={(e) => updateActionField('amount', e.target.value)}
                  placeholder="50"
                />
              </Field>

              <Field>
                <FieldLabel>{t('campaigns.form.executeOrderLabel', { defaultValue: 'Execution order' })}</FieldLabel>
                <Input
                  type="number"
                  value={formValues.action.executeOrder}
                  disabled
                  readOnly
                />
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>

      <div className="flex flex-col gap-3 border-t border-border pt-6 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <InfoIcon size={16} className="shrink-0 text-primary" />
          <span>{t('campaigns.form.placeholderNotice', { defaultValue: 'Campaign creation will be available in a future update.' })}</span>
        </div>

        <div className="flex items-center gap-3">
          <Button type="button" variant="outline" onClick={onCancel}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" disabled>
            {t('campaigns.form.saveDraft')}
          </Button>
        </div>
      </div>
    </form>
  )
}
