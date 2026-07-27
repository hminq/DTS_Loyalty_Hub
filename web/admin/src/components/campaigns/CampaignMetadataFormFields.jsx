import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Combobox } from '../ui/combobox'
import { DateTimePicker } from '../ui/date-time-picker'
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { CampaignBannerField } from './CampaignBannerField'

export function CampaignMetadataFormFields({
  formValues,
  options = {},
  canUploadBanner = false,
  isSubmitting = false,
  fieldErrors = {},
  updateField,
  handleEventTypeChange,
  handleConditionOptionChange,
  t,
}) {
  const selectedEvent = (options.eventTypes || []).find((e) => e.value === formValues.eventType)
  const conditionOptions = selectedEvent?.conditionOptions || []

  const timeZoneText = t('campaigns.form.timeZoneHelper', {
    timeZone: options.schedule?.timeZone || 'UTC',
    defaultValue: `Schedule operates in ${options.schedule?.timeZone || 'UTC'} timezone.`,
  })

  const weekdayLabels = (options.schedule?.daysOfWeek || []).map((day) => day.label).join(', ')

  return (
    <>
      {/* General information */}
      <Card>
        <CardHeader>
          <CardTitle>
            {t('campaigns.form.generalTitle', { defaultValue: 'General information' })}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 lg:grid-cols-[minmax(0,1.35fr)_minmax(300px,0.65fr)]">
            <FieldGroup>
              <Field invalid={Boolean(fieldErrors.campaignName)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.campaignNameLabel', { defaultValue: 'Campaign name' })}
                </FieldLabel>
                <Input
                  value={formValues.campaignName}
                  onChange={(e) => updateField('campaignName', e.target.value)}
                  placeholder={t('campaigns.form.campaignNamePlaceholder', {
                    defaultValue: 'Enter campaign name',
                  })}
                  aria-invalid={Boolean(fieldErrors.campaignName)}
                />
                {fieldErrors.campaignName ? (
                  <FieldError>{fieldErrors.campaignName}</FieldError>
                ) : null}
              </Field>

              <Field invalid={Boolean(fieldErrors.description)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.descriptionLabel', { defaultValue: 'Description' })}
                </FieldLabel>
                <Input
                  value={formValues.description}
                  onChange={(e) => updateField('description', e.target.value)}
                  placeholder={t('campaigns.form.descriptionPlaceholder', {
                    defaultValue: 'Describe the campaign purpose and rewards',
                  })}
                  aria-invalid={Boolean(fieldErrors.description)}
                />
                {fieldErrors.description ? (
                  <FieldError>{fieldErrors.description}</FieldError>
                ) : null}
              </Field>

              <div className="grid gap-4 sm:grid-cols-2">
                <Field invalid={Boolean(fieldErrors.eventType)} disabled={isSubmitting}>
                  <FieldLabel>
                    {t('campaigns.form.eventTypeLabel', { defaultValue: 'Event type' })}
                  </FieldLabel>
                  <Combobox
                    value={formValues.eventType}
                    onValueChange={handleEventTypeChange}
                    options={options.eventTypes ?? []}
                    placeholder={t('campaigns.form.selectEventType', {
                      defaultValue: 'Select event type',
                    })}
                    emptyOptionLabel={t('campaigns.form.selectEventType', {
                      defaultValue: 'Select event type',
                    })}
                    ariaLabel={t('campaigns.form.eventTypeLabel', {
                      defaultValue: 'Event type',
                    })}
                    disabled={isSubmitting}
                  />
                  {fieldErrors.eventType ? (
                    <FieldError>{fieldErrors.eventType}</FieldError>
                  ) : null}
                </Field>

                <Field
                  invalid={Boolean(fieldErrors.conditionOptionCode || fieldErrors.condition)}
                  disabled={!formValues.eventType || isSubmitting}
                >
                  <FieldLabel>
                    {t('campaigns.form.conditionLabel', {
                      defaultValue: 'Campaign condition',
                    })}
                  </FieldLabel>
                  <Combobox
                    value={formValues.conditionOptionCode}
                    onValueChange={handleConditionOptionChange}
                    options={conditionOptions}
                    placeholder={t('campaigns.form.selectCondition', {
                      defaultValue: 'Select campaign condition',
                    })}
                    emptyOptionLabel={t('campaigns.form.selectCondition', {
                      defaultValue: 'Select campaign condition',
                    })}
                    disabled={!formValues.eventType || isSubmitting}
                    ariaLabel={t('campaigns.form.conditionLabel', {
                      defaultValue: 'Campaign condition',
                    })}
                  />
                  {fieldErrors.conditionOptionCode || fieldErrors.condition ? (
                    <FieldError>
                      {fieldErrors.conditionOptionCode || fieldErrors.condition}
                    </FieldError>
                  ) : null}
                </Field>
              </div>
            </FieldGroup>

            <Field
              invalid={Boolean(fieldErrors.bannerImageUrl)}
              disabled={!canUploadBanner || isSubmitting}
            >
              <FieldLabel>
                {t('campaigns.form.bannerLabel', { defaultValue: 'Campaign banner' })}
              </FieldLabel>
              <CampaignBannerField
                file={formValues.bannerFile}
                existingUrl={formValues.bannerImageUrl}
                onChange={(file) => updateField('bannerFile', file)}
                onClear={() => {
                  updateField('bannerFile', null)
                  updateField('bannerImageKey', '')
                  updateField('bannerImageUrl', '')
                }}
                disabled={!canUploadBanner || isSubmitting}
                error={fieldErrors.bannerImageUrl || ''}
                t={t}
              />
            </Field>
          </div>
        </CardContent>
      </Card>

      {/* Schedule and active range */}
      <Card>
        <CardHeader>
          <CardTitle>
            {t('campaigns.form.scheduleTitle', { defaultValue: 'Schedule & active range' })}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field invalid={Boolean(fieldErrors.startDate)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
                </FieldLabel>
                <DateTimePicker
                  value={formValues.startDate}
                  onChange={(val) => updateField('startDate', val)}
                  ariaLabel={t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
                  disabled={isSubmitting}
                />
                {fieldErrors.startDate ? (
                  <FieldError>{fieldErrors.startDate}</FieldError>
                ) : null}
              </Field>

              <Field invalid={Boolean(fieldErrors.endDate)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
                </FieldLabel>
                <DateTimePicker
                  value={formValues.endDate}
                  onChange={(val) => updateField('endDate', val)}
                  ariaLabel={t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
                  disabled={isSubmitting}
                />
                {fieldErrors.endDate ? <FieldError>{fieldErrors.endDate}</FieldError> : null}
              </Field>
              <Field invalid={Boolean(fieldErrors.scheduleCron)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.scheduleCronLabel', { defaultValue: 'Schedule CRON' })}
                </FieldLabel>
                <Input
                  value={formValues.scheduleCron}
                  onChange={(e) => updateField('scheduleCron', e.target.value)}
                  placeholder="0 0 2 * * ?"
                  aria-invalid={Boolean(fieldErrors.scheduleCron)}
                  disabled={isSubmitting}
                />
                {fieldErrors.scheduleCron ? (
                  <FieldError>{fieldErrors.scheduleCron}</FieldError>
                ) : null}
              </Field>

              <Field invalid={Boolean(fieldErrors.durationHour)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.durationHourLabel', { defaultValue: 'Duration (hours)' })}
                </FieldLabel>
                <Input
                  type="number"
                  min="1"
                  value={formValues.durationHour}
                  onChange={(e) => updateField('durationHour', e.target.value)}
                  placeholder="2"
                  aria-invalid={Boolean(fieldErrors.durationHour)}
                  disabled={isSubmitting}
                />
                {fieldErrors.durationHour ? (
                  <FieldError>{fieldErrors.durationHour}</FieldError>
                ) : null}
              </Field>
            </div>
            <FieldDescription>{timeZoneText}</FieldDescription>
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

      {/* Limits */}
      <Card>
        <CardHeader>
          <CardTitle>{t('campaigns.form.limitsTitle', { defaultValue: 'Limits' })}</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field invalid={Boolean(fieldErrors.userLimitTotal)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.userLimitTotalLabel', {
                    defaultValue: 'Max rewards per customer',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0"
                  value={formValues.userLimitTotal}
                  onChange={(e) => updateField('userLimitTotal', e.target.value)}
                  placeholder="1"
                  aria-invalid={Boolean(fieldErrors.userLimitTotal)}
                  disabled={isSubmitting}
                />
                <FieldDescription>
                  {t('campaigns.form.userLimitTotalHelper', {
                    defaultValue:
                      'Maximum times one customer can receive this campaign overall. Leave empty for unlimited.',
                  })}
                </FieldDescription>
                {fieldErrors.userLimitTotal ? (
                  <FieldError>{fieldErrors.userLimitTotal}</FieldError>
                ) : null}
              </Field>

              <Field invalid={Boolean(fieldErrors.userLimitSession)} disabled={isSubmitting}>
                <FieldLabel>
                  {t('campaigns.form.userLimitSessionLabel', {
                    defaultValue: 'Max rewards per customer per session',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0"
                  value={formValues.userLimitSession}
                  onChange={(e) => updateField('userLimitSession', e.target.value)}
                  placeholder="1"
                  aria-invalid={Boolean(fieldErrors.userLimitSession)}
                  disabled={isSubmitting}
                />
                <FieldDescription>
                  {t('campaigns.form.userLimitSessionHelper', {
                    defaultValue:
                      'Maximum times one customer can receive this campaign in each session. Leave empty for unlimited.',
                  })}
                </FieldDescription>
                {fieldErrors.userLimitSession ? (
                  <FieldError>{fieldErrors.userLimitSession}</FieldError>
                ) : null}
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>
    </>
  )
}
