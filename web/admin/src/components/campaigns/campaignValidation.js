function validateActionLimits(action = {}, prefix = '', t) {
  const errors = {}
  const keyTotal = prefix ? `${prefix}.totalCount` : 'totalCount'
  const keySession = prefix ? `${prefix}.sessionCount` : 'sessionCount'

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

function validateSingleAction(action = {}, index, t) {
  const errors = {}
  const prefix = `actions[${index}]`

  if (!action.actionType) {
    errors[`${prefix}.actionType`] = t('campaigns.errors.actionTypeRequired', {
      defaultValue: 'Action type is required.',
    })
  }

  if (!action.calculationType) {
    errors[`${prefix}.calculationType`] = t('campaigns.errors.calculationTypeRequired', {
      defaultValue: 'Calculation type is required.',
    })
  }

  if (!action.recipient) {
    errors[`${prefix}.recipient`] = t('campaigns.errors.recipientRequired', {
      defaultValue: 'Recipient is required.',
    })
  }

  const amountVal = Number(action.amount)
  if (
    action.amount === '' ||
    action.amount == null ||
    Number.isNaN(amountVal) ||
    amountVal <= 0
  ) {
    errors[`${prefix}.amount`] = t('campaigns.errors.amountRequired', {
      defaultValue: 'Reward amount must be greater than zero.',
    })
  } else {
    const parts = String(action.amount).split('.')
    if (parts.length > 1 && parts[1].length > 2) {
      errors[`${prefix}.amount`] = t('campaigns.errors.amountInvalid', {
        defaultValue: 'Reward amount can have at most two decimal places.',
      })
    }
  }

  Object.assign(errors, validateActionLimits(action, prefix, t))

  return errors
}

export function validateCampaignMetadata(formValues = {}, t) {
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

  if (!formValues.conditionOptionCode) {
    errors.conditionOptionCode = t('campaigns.errors.conditionRequired', {
      defaultValue: 'Campaign condition is required.',
    })
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
  if (total !== null && (!Number.isInteger(total) || total < 0)) {
    errors.userLimitTotal = t('campaigns.errors.userLimitTotalInvalid', {
      defaultValue: 'Total limit must be a non-negative integer.',
    })
  }

  const session =
    formValues.userLimitSession !== '' && formValues.userLimitSession != null
      ? Number(formValues.userLimitSession)
      : null
  if (session !== null && (!Number.isInteger(session) || session < 0)) {
    errors.userLimitSession = t('campaigns.errors.userLimitSessionInvalid', {
      defaultValue: 'Session limit must be a non-negative integer.',
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

export function validateCampaignCreate(formValues = {}, t) {
  const metadataVal = validateCampaignMetadata(formValues, t)
  const errors = { ...metadataVal.errors }

  const actions = formValues.actions || []
  if (actions.length === 0) {
    errors.actions = t('campaigns.errors.actionsRequired', {
      defaultValue: 'At least one action is required.',
    })
  }

  for (let i = 0; i < actions.length; i++) {
    Object.assign(errors, validateSingleAction(actions[i], i, t))
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}

export function validateCampaignAction(actionValues = {}, t) {
  const errors = {}

  if (!actionValues.actionType) {
    errors.actionType = t('campaigns.errors.actionTypeRequired', {
      defaultValue: 'Action type is required.',
    })
  }

  if (!actionValues.calculationType) {
    errors.calculationType = t('campaigns.errors.calculationTypeRequired', {
      defaultValue: 'Calculation type is required.',
    })
  }

  if (!actionValues.recipient) {
    errors.recipient = t('campaigns.errors.recipientRequired', {
      defaultValue: 'Recipient is required.',
    })
  }

  const amountVal = Number(actionValues.amount)
  if (
    actionValues.amount === '' ||
    actionValues.amount == null ||
    Number.isNaN(amountVal) ||
    amountVal <= 0
  ) {
    errors.amount = t('campaigns.errors.amountRequired', {
      defaultValue: 'Reward amount must be greater than zero.',
    })
  } else {
    const parts = String(actionValues.amount).split('.')
    if (parts.length > 1 && parts[1].length > 2) {
      errors.amount = t('campaigns.errors.amountInvalid', {
        defaultValue: 'Reward amount can have at most two decimal places.',
      })
    }
  }

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

  Object.assign(errors, validateActionLimits(actionValues, '', t))

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}
