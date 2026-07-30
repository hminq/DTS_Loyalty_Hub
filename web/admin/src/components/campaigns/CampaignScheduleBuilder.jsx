import {
  CalendarBlankIcon,
  CalendarDotIcon,
  CalendarDotsIcon,
  CalendarStarIcon,
  CodeBlockIcon,
  WarningIcon,
} from '@phosphor-icons/react'
import { useState } from 'react'

import { Alert, AlertDescription, AlertTitle } from '../ui/alert'
import { Badge } from '../ui/badge'
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { ToggleGroup, ToggleGroupItem } from '../ui/toggle-group'
import {
  buildCampaignScheduleCron,
  CAMPAIGN_SCHEDULE_MODES,
  CAMPAIGN_SCHEDULE_WEEKDAYS,
  getMonthlyScheduleWarnings,
  parseCampaignScheduleCron,
} from './campaignSchedule'

const MODE_ICONS = {
  [CAMPAIGN_SCHEDULE_MODES.DAILY]: CalendarBlankIcon,
  [CAMPAIGN_SCHEDULE_MODES.WEEKLY]: CalendarDotsIcon,
  [CAMPAIGN_SCHEDULE_MODES.MONTHLY]: CalendarDotIcon,
  [CAMPAIGN_SCHEDULE_MODES.LAST_DAY]: CalendarStarIcon,
  [CAMPAIGN_SCHEDULE_MODES.CUSTOM]: CodeBlockIcon,
}

function getModeOptions(t) {
  return [
    {
      value: CAMPAIGN_SCHEDULE_MODES.DAILY,
      label: t('campaigns.form.scheduleModeDaily', { defaultValue: 'Daily' }),
    },
    {
      value: CAMPAIGN_SCHEDULE_MODES.WEEKLY,
      label: t('campaigns.form.scheduleModeWeekly', { defaultValue: 'Weekly' }),
    },
    {
      value: CAMPAIGN_SCHEDULE_MODES.MONTHLY,
      label: t('campaigns.form.scheduleModeMonthly', { defaultValue: 'Monthly' }),
    },
    {
      value: CAMPAIGN_SCHEDULE_MODES.LAST_DAY,
      label: t('campaigns.form.scheduleModeLastDay', { defaultValue: 'Last day' }),
    },
    {
      value: CAMPAIGN_SCHEDULE_MODES.CUSTOM,
      label: t('campaigns.form.scheduleModeCustom', { defaultValue: 'Cron' }),
    },
  ]
}

function warningMessage(code, t) {
  if (code === 'FEBRUARY_NON_LEAP') {
    return t('campaigns.form.scheduleWarningDay29', {
      defaultValue: 'Day 29 will be skipped in February except during leap years.',
    })
  }

  if (code === 'FEBRUARY') {
    return t('campaigns.form.scheduleWarningDay30', {
      defaultValue: 'Day 30 will be skipped in February.',
    })
  }

  if (code === 'SHORT_MONTHS') {
    return t('campaigns.form.scheduleWarningDay31', {
      defaultValue:
        'Day 31 will be skipped in February, April, June, September, and November.',
    })
  }

  return ''
}

export function CampaignScheduleBuilder({
  value,
  onChange,
  daysOfWeek = [],
  timeZone = 'UTC',
  disabled = false,
  error = '',
  t,
}) {
  const parsedSchedule = parseCampaignScheduleCron(value)
  const [selectedMode, setSelectedMode] = useState(parsedSchedule.mode)
  const modeOptions = getModeOptions(t)
  const availableDays = daysOfWeek.length > 0
    ? daysOfWeek
    : CAMPAIGN_SCHEDULE_WEEKDAYS.map((day) => ({ value: day, label: day }))
  const monthlyWarnings =
    selectedMode === CAMPAIGN_SCHEDULE_MODES.MONTHLY
      ? getMonthlyScheduleWarnings(parsedSchedule.daysOfMonth)
      : []

  const updateGeneratedCron = (overrides = {}) => {
    const nextCron = buildCampaignScheduleCron({
      mode: selectedMode,
      time: parsedSchedule.time,
      weekdays: parsedSchedule.weekdays,
      daysOfMonth: parsedSchedule.daysOfMonth,
      ...overrides,
    })

    if (nextCron) onChange(nextCron)
  }

  const handleModeChange = (nextValues) => {
    const nextMode = nextValues[0]
    if (!nextMode) return

    setSelectedMode(nextMode)
    if (nextMode === CAMPAIGN_SCHEDULE_MODES.CUSTOM) return

    const nextCron = buildCampaignScheduleCron({
      mode: nextMode,
      time: parsedSchedule.time,
      weekdays:
        parsedSchedule.weekdays.length > 0
          ? parsedSchedule.weekdays
          : [availableDays[0]?.value || CAMPAIGN_SCHEDULE_WEEKDAYS[0]],
      daysOfMonth:
        parsedSchedule.daysOfMonth.length > 0
          ? parsedSchedule.daysOfMonth
          : [1],
    })
    if (nextCron) onChange(nextCron)
  }

  const handleWeekdaysChange = (nextDays) => {
    if (nextDays.length === 0) return
    updateGeneratedCron({ weekdays: nextDays })
  }

  const handleMonthDaysChange = (nextDays) => {
    if (nextDays.length === 0) return
    updateGeneratedCron({ daysOfMonth: nextDays.map(Number) })
  }

  return (
    <Field invalid={Boolean(error)} disabled={disabled}>
      <FieldLabel>
        {t('campaigns.form.schedulePatternLabel', { defaultValue: 'Repeat' })}
      </FieldLabel>
      <ToggleGroup
        value={[selectedMode]}
        onValueChange={handleModeChange}
        variant="outline"
        disabled={disabled}
        aria-label={t('campaigns.form.schedulePatternLabel', { defaultValue: 'Repeat' })}
        className="grid w-full grid-cols-2 sm:grid-cols-3 xl:grid-cols-5"
      >
        {modeOptions.map((option) => {
          const Icon = MODE_ICONS[option.value]
          return (
            <ToggleGroupItem key={option.value} value={option.value} className="w-full">
              <Icon aria-hidden="true" />
              {option.label}
            </ToggleGroupItem>
          )
        })}
      </ToggleGroup>

      {selectedMode === CAMPAIGN_SCHEDULE_MODES.CUSTOM ? (
        <FieldGroup className="mt-2">
          <Field invalid={Boolean(error)} disabled={disabled}>
            <FieldLabel htmlFor="campaign-schedule-cron">
              {t('campaigns.form.scheduleCronLabel', { defaultValue: 'Schedule CRON' })}
            </FieldLabel>
            <Input
              id="campaign-schedule-cron"
              value={value}
              onChange={(event) => onChange(event.target.value)}
              placeholder="0 0 9 15 * ?"
              aria-invalid={Boolean(error)}
              disabled={disabled}
            />
            <FieldDescription>
              {t('campaigns.form.scheduleCronHelper', {
                defaultValue:
                  'Quartz format in UTC. Supported patterns: daily, selected weekdays, selected days of month, and last day of month.',
              })}
            </FieldDescription>
          </Field>
        </FieldGroup>
      ) : (
        <FieldGroup className="mt-2">
          <div className="grid gap-4 sm:grid-cols-2">
            <Field disabled={disabled}>
              <FieldLabel htmlFor="campaign-schedule-time">
                {t('campaigns.form.scheduleTimeLabel', { defaultValue: 'Run at' })}
              </FieldLabel>
              <Input
                id="campaign-schedule-time"
                type="time"
                value={parsedSchedule.time}
                onChange={(event) => updateGeneratedCron({ time: event.target.value })}
                disabled={disabled}
              />
            </Field>
          </div>

          {selectedMode === CAMPAIGN_SCHEDULE_MODES.MONTHLY ? (
            <Field disabled={disabled}>
              <FieldLabel>
                {t('campaigns.form.scheduleDaysOfMonthLabel', {
                  defaultValue: 'Days of month',
                })}
              </FieldLabel>
              <ToggleGroup
                value={parsedSchedule.daysOfMonth.map(String)}
                onValueChange={handleMonthDaysChange}
                multiple
                variant="outline"
                size="sm"
                disabled={disabled}
                aria-label={t('campaigns.form.scheduleDaysOfMonthLabel', {
                  defaultValue: 'Days of month',
                })}
                className="grid w-full grid-cols-7"
              >
                {Array.from({ length: 31 }, (_, index) => index + 1).map((day) => (
                  <ToggleGroupItem key={day} value={String(day)} className="w-full px-1">
                    {day}
                  </ToggleGroupItem>
                ))}
              </ToggleGroup>
              <FieldDescription>
                {t('campaigns.form.scheduleDaysOfMonthHelper', {
                  defaultValue: 'Select one or more calendar days.',
                })}
              </FieldDescription>
            </Field>
          ) : null}

          {selectedMode === CAMPAIGN_SCHEDULE_MODES.WEEKLY ? (
            <Field disabled={disabled}>
              <FieldLabel>
                {t('campaigns.form.scheduleWeekdaysLabel', {
                  defaultValue: 'Days of week',
                })}
              </FieldLabel>
              <ToggleGroup
                value={parsedSchedule.weekdays}
                onValueChange={handleWeekdaysChange}
                multiple
                variant="outline"
                size="sm"
                disabled={disabled}
                aria-label={t('campaigns.form.scheduleWeekdaysLabel', {
                  defaultValue: 'Days of week',
                })}
                className="grid w-full grid-cols-4 sm:grid-cols-7"
              >
                {availableDays.map((day) => (
                  <ToggleGroupItem key={day.value} value={day.value} className="w-full">
                    {day.label}
                  </ToggleGroupItem>
                ))}
              </ToggleGroup>
            </Field>
          ) : null}
        </FieldGroup>
      )}

      {monthlyWarnings.length > 0 ? (
        <Alert variant="warning" className="mt-2">
          <WarningIcon weight="fill" aria-hidden="true" />
          <AlertTitle>
            {t('campaigns.form.scheduleWarningTitle', { defaultValue: 'Calendar warning' })}
          </AlertTitle>
          <AlertDescription className="grid gap-1">
            {monthlyWarnings.map((warning) => (
              <p key={warning}>{warningMessage(warning, t)}</p>
            ))}
          </AlertDescription>
        </Alert>
      ) : null}

      <div className="mt-2 flex min-w-0 flex-wrap items-center gap-2 rounded-md border bg-muted/40 px-3 py-2">
        <Badge variant="secondary">
          {t('campaigns.form.scheduleTimeZoneBadge', {
            timeZone,
            defaultValue: `Timezone: ${timeZone}`,
          })}
        </Badge>
        <code className="min-w-0 break-all text-xs text-foreground">{value || '—'}</code>
      </div>

      {error ? <FieldError>{error}</FieldError> : null}
    </Field>
  )
}
