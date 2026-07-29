import { isTargetCompatible } from './campaignCompatibility.js'

const CAMPAIGN_SCHEDULE_WEEKDAYS = [
  'MON',
  'TUE',
  'WED',
  'THU',
  'FRI',
  'SAT',
  'SUN',
]

function toFieldKey(prefix, field) {
  return prefix ? `${prefix}.${field}` : field
}

function isCanonicalCronNumber(value, minimum, maximum) {
  const parsed = Number(value)
  return (
    Number.isInteger(parsed) &&
    parsed >= minimum &&
    parsed <= maximum &&
    value === String(parsed)
  )
}

export function isValidCampaignScheduleCron(value) {
  if (typeof value !== 'string' || !value.trim()) return false

  const parts = value.trim().split(/\s+/)
  if (parts.length !== 6) return false

  const [second, minute, hour, dayOfMonth, month, dayOfWeek] = parts
  if (
    second !== '0' ||
    !isCanonicalCronNumber(minute, 0, 59) ||
    !isCanonicalCronNumber(hour, 0, 23)
  ) {
    return false
  }

  if (dayOfMonth === '*' && month === '*' && dayOfWeek === '?') {
    return true
  }

  if (dayOfMonth !== '?' || month !== '*') return false

  const weekdays = dayOfWeek.split(',')
  if (weekdays.length === 0 || weekdays.some((day) => !day)) return false

  let previousIndex = -1
  for (const weekday of weekdays) {
    const currentIndex = CAMPAIGN_SCHEDULE_WEEKDAYS.indexOf(weekday)
    if (currentIndex === -1 || currentIndex <= previousIndex) return false
    previousIndex = currentIndex
  }

  return true
}

function validateActionLimits(action = {}, prefix = '', t) {
  const errors = {}
  const keyTotal = toFieldKey(prefix, 'totalCount')
  const keySession = toFieldKey(prefix, 'sessionCount')

  const total =
    action.totalCount !== '' && action.totalCount != null
      ? Number(action.totalCount)
      : null
  if (total !== null && (!Number.isInteger(total) || total < 0)) {
    errors[keyTotal] = t('campaigns.errors.actionLimitTotalInvalid', {
      defaultValue: 'Action total limit must be a non-negative integer.',
    })
  }

  const session =
    action.sessionCount !== '' && action.sessionCount != null
      ? Number(action.sessionCount)
      : null
  if (session !== null && (!Number.isInteger(session) || session < 0)) {
    errors[keySession] = t('campaigns.errors.actionLimitSessionInvalid', {
      defaultValue: 'Action session limit must be a non-negative integer.',
    })
  }

  if (
    total !== null &&
    session !== null &&
    Number.isInteger(total) &&
    Number.isInteger(session) &&
    session > total
  ) {
    errors[keySession] = t('campaigns.errors.actionLimitSessionExceedsTotal', {
      defaultValue:
        'Per-session action limit cannot exceed the overall action limit.',
    })
  }

  return errors
}

function getDecimalScale(value) {
  const match = String(value).trim().match(/^[+-]?\d+(?:\.(\d+))?$/)
  return match ? (match[1]?.length ?? 0) : null
}

function validateSingleAction(
  action = {},
  prefix = '',
  options = {},
  campaignContext = {},
  t,
) {
  const errors = {}
  const actionTypeKey = toFieldKey(prefix, 'actionType')
  const targetSelectorKey = toFieldKey(prefix, 'targetSelector')

  if (!action.actionType) {
    errors[actionTypeKey] = t('campaigns.errors.actionTypeRequired', {
      defaultValue: 'Action type is required.',
    })
  }

  if (!action.targetSelector) {
    errors[targetSelectorKey] = t('campaigns.errors.targetSelectorRequired', {
      defaultValue: 'Target is required.',
    })
  }

  const selectedActionDef = (options.actionTypes || []).find(
    (actionType) => actionType.value === action.actionType,
  )
  if (action.actionType && !selectedActionDef) {
    errors[actionTypeKey] = t('campaigns.errors.actionTypeUnsupported', {
      defaultValue: 'The selected action type is not supported.',
    })
  }

  const selectedEvent = (options.eventTypes || []).find(
    (event) => event.value === campaignContext.eventType,
  )
  const selectedConditionPreset = (selectedEvent?.conditionPresets || []).find(
    (preset) => preset.value === campaignContext.conditionPresetCode,
  )
  const selectedTarget = (selectedEvent?.targets || []).find(
    (target) => target.value === action.targetSelector,
  )

  if (
    action.targetSelector &&
    (!selectedTarget ||
      !isTargetCompatible(selectedTarget, selectedActionDef, selectedConditionPreset))
  ) {
    errors[targetSelectorKey] = t('campaigns.errors.targetSelectorIncompatible', {
      defaultValue: 'The selected target is not compatible with this campaign condition.',
    })
  }

  if (selectedActionDef) {
    for (const paramDef of selectedActionDef.parameters || []) {
      const fieldKey = toFieldKey(prefix, `parameters.${paramDef.code}`)
      const value = action.parameters?.[paramDef.code]

      if (paramDef.dataType !== 'DECIMAL') {
        errors[fieldKey] = t('campaigns.errors.parameterTypeUnsupported', {
          type: paramDef.dataType,
          defaultValue: `Parameter type ${paramDef.dataType} is not supported.`,
        })
        continue
      }

      if (paramDef.required && (value === '' || value == null)) {
        errors[fieldKey] = t('campaigns.errors.parameterRequired', {
          defaultValue: `${paramDef.label || paramDef.code} is required.`,
        })
      } else if (value !== '' && value != null) {
        const numericValue = Number(value)
        const decimalScale = getDecimalScale(value)

        if (!Number.isFinite(numericValue) || decimalScale === null) {
          errors[fieldKey] = t('campaigns.errors.parameterInvalidNumber', {
            defaultValue: 'Must be a valid number.',
          })
        } else if (
          Number.isInteger(paramDef.scale) &&
          decimalScale > paramDef.scale
        ) {
          errors[fieldKey] = t('campaigns.errors.parameterScaleExceeded', {
            scale: paramDef.scale,
            defaultValue: `Must have at most ${paramDef.scale} decimal places.`,
          })
        } else if (
          paramDef.minimumExclusive != null &&
          numericValue <= paramDef.minimumExclusive
        ) {
          errors[fieldKey] = t('campaigns.errors.parameterTooSmall', {
            minimum: paramDef.minimumExclusive,
            defaultValue: `Must be greater than ${paramDef.minimumExclusive}.`,
          })
        } else if (paramDef.maximum != null && numericValue > paramDef.maximum) {
          errors[fieldKey] = t('campaigns.errors.parameterTooLarge', {
            maximum: paramDef.maximum,
            defaultValue: `Must be at most ${paramDef.maximum}.`,
          })
        }
      }
    }
  }

  Object.assign(errors, validateActionLimits(action, prefix, t))

  return errors
}

export function validateCampaignMetadata(formValues = {}, options = {}, t) {
  const errors = {}

  const campaignName = formValues.campaignName?.trim() || ''
  if (!campaignName) {
    errors.campaignName = t('campaigns.errors.campaignNameRequired', {
      defaultValue: 'Campaign name is required.',
    })
  } else if (campaignName.length > 200) {
    errors.campaignName = t('campaigns.errors.campaignNameTooLong', {
      defaultValue: 'Campaign name cannot exceed 200 characters.',
    })
  }

  if (!formValues.eventType) {
    errors.eventType = t('campaigns.errors.eventTypeRequired', {
      defaultValue: 'Event type is required.',
    })
  }

  if (!formValues.conditionPresetCode) {
    errors.conditionPresetCode = t('campaigns.errors.conditionRequired', {
      defaultValue: 'Campaign condition is required.',
    })
  } else {
    const selectedEvent = (options.eventTypes || []).find(
      (event) => event.value === formValues.eventType,
    )
    const selectedConditionPreset = (selectedEvent?.conditionPresets || []).find(
      (preset) => preset.value === formValues.conditionPresetCode,
    )
    if (!selectedConditionPreset) {
      errors.conditionPresetCode = t('campaigns.errors.conditionUnsupported', {
        defaultValue: 'The selected campaign condition is not supported.',
      })
    }
  }

  const start = formValues.startDate ? new Date(formValues.startDate) : null
  const end = formValues.endDate ? new Date(formValues.endDate) : null

  if (!start || Number.isNaN(start.getTime())) {
    errors.startDate = t('campaigns.errors.startDateRequired', {
      defaultValue: 'Start date is required.',
    })
  }
  if (!end || Number.isNaN(end.getTime())) {
    errors.endDate = t('campaigns.errors.endDateRequired', {
      defaultValue: 'End date is required.',
    })
  }
  if (start && end && !Number.isNaN(start.getTime()) && !Number.isNaN(end.getTime())) {
    if (end.getTime() <= start.getTime()) {
      errors.endDate = t('campaigns.errors.endDateBeforeStart', {
        defaultValue: 'End date must be later than start date.',
      })
    }
  }

  const scheduleCron = formValues.scheduleCron?.trim() || ''
  if (!scheduleCron) {
    errors.scheduleCron = t('campaigns.errors.scheduleCronRequired', {
      defaultValue: 'Schedule CRON expression is required.',
    })
  } else if (scheduleCron.length > 100) {
    errors.scheduleCron = t('campaigns.errors.scheduleCronTooLong', {
      defaultValue: 'Schedule CRON expression cannot exceed 100 characters.',
    })
  } else if (!isValidCampaignScheduleCron(scheduleCron)) {
    errors.scheduleCron = t('campaigns.errors.scheduleCronInvalid', {
      defaultValue:
        'Use a supported CRON format, for example 0 42 15 * * ? or 0 42 15 ? * MON,WED,SAT.',
    })
  }

  const durationHour = Number(formValues.durationHour)
  if (
    formValues.durationHour === '' ||
    formValues.durationHour == null ||
    !Number.isInteger(durationHour) ||
    durationHour <= 0
  ) {
    errors.durationHour = t('campaigns.errors.durationHourInvalid', {
      defaultValue: 'Duration must be a positive integer.',
    })
  }

  const total =
    formValues.userLimitTotal !== '' && formValues.userLimitTotal != null
      ? Number(formValues.userLimitTotal)
      : null
  if (total !== null && (!Number.isInteger(total) || total <= 0)) {
    errors.userLimitTotal = t('campaigns.errors.userLimitTotalInvalid', {
      defaultValue: 'Total limit must be a positive integer.',
    })
  }

  const session =
    formValues.userLimitSession !== '' && formValues.userLimitSession != null
      ? Number(formValues.userLimitSession)
      : null
  if (session !== null && (!Number.isInteger(session) || session <= 0)) {
    errors.userLimitSession = t('campaigns.errors.userLimitSessionInvalid', {
      defaultValue: 'Session limit must be a positive integer.',
    })
  }

  if (
    total !== null &&
    session !== null &&
    Number.isInteger(total) &&
    Number.isInteger(session) &&
    session > total
  ) {
    errors.userLimitSession = t('campaigns.errors.userLimitSessionExceedsTotal', {
      defaultValue: 'Session limit cannot exceed total limit.',
    })
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}

export function validateCampaignCreate(formValues = {}, options = {}, t) {
  const metadataVal = validateCampaignMetadata(formValues, options, t)
  const errors = { ...metadataVal.errors }

  const actions = formValues.actions || []
  if (actions.length === 0) {
    errors.actions = t('campaigns.errors.actionsRequired', {
      defaultValue: 'At least one action is required.',
    })
  }

  for (let i = 0; i < actions.length; i++) {
    Object.assign(
      errors,
      validateSingleAction(
        actions[i],
        `actions[${i}]`,
        options,
        {
          eventType: formValues.eventType,
          conditionPresetCode: formValues.conditionPresetCode,
        },
        t,
      ),
    )
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}

export function validateCampaignAction(
  actionValues = {},
  options = {},
  campaignContext = {},
  t,
) {
  const errors = validateSingleAction(
    actionValues,
    '',
    options,
    campaignContext,
    t,
  )

  const orderVal = Number(actionValues.executeOrder)
  if (
    actionValues.executeOrder === '' ||
    actionValues.executeOrder == null ||
    !Number.isInteger(orderVal) ||
    orderVal <= 0
  ) {
    errors.executeOrder = t('campaigns.errors.executeOrderInvalid', {
      defaultValue: 'Execution order must be a positive integer.',
    })
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}
