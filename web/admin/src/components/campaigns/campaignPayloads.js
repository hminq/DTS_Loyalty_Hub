export function buildCampaignActionPayload(actionValues = {}, executeOrder = 1) {
  const actionType = actionValues.actionType || null
  const calculationType = actionValues.calculationType || null
  const recipient = actionValues.recipient || null

  const amount =
    actionValues.amount !== '' && actionValues.amount != null ? Number(actionValues.amount) : null

  return {
    actionType,
    actionConfig: {
      calculationType,
      recipient,
      amount,
      calculationBase: null,
      percentage: null,
      maximumPoints: null,
    },
    executeOrder,
    totalCount: null,
    sessionCount: null,
    totalAmount: null,
    sessionAmount: null,
  }
}

function buildCampaignMetadataPayload(formValues = {}, options = {}) {
  const campaignName = formValues.campaignName?.trim() || null
  const description = formValues.description?.trim() || null
  const bannerImageUrl = formValues.bannerImageUrl || null
  const eventType = formValues.eventType || null
  const scheduleCron = formValues.scheduleCron?.trim() || null

  const selectedEvent = (options.eventTypes || []).find(
    (option) => option.value === eventType,
  )
  const selectedCondition = (selectedEvent?.conditionOptions || []).find(
    (option) => option.value === formValues.conditionOptionCode,
  )
  const sources = selectedCondition?.sources || []
  const condition = { sources }

  const startDate = formValues.startDate ? new Date(formValues.startDate).toISOString() : null
  const endDate = formValues.endDate ? new Date(formValues.endDate).toISOString() : null

  const durationHour =
    formValues.durationHour !== '' && formValues.durationHour != null
      ? Number(formValues.durationHour)
      : null

  const userLimitTotal =
    formValues.userLimitTotal !== '' && formValues.userLimitTotal != null
      ? Number(formValues.userLimitTotal)
      : null

  const userLimitSession =
    formValues.userLimitSession !== '' && formValues.userLimitSession != null
      ? Number(formValues.userLimitSession)
      : null

  return {
    campaignName,
    description,
    bannerImageUrl,
    eventType,
    condition,
    startDate,
    endDate,
    scheduleCron,
    durationHour,
    userLimitTotal,
    userLimitSession,
  }
}

export function buildCampaignCreatePayload(formValues = {}, options = {}) {
  const metadata = buildCampaignMetadataPayload(formValues, options)
  const actions = (formValues.actions || []).map((action, index) =>
    buildCampaignActionPayload(action, index + 1),
  )

  return {
    ...metadata,
    actions,
  }
}

export function buildCampaignUpdatePayload(formValues = {}, options = {}) {
  return buildCampaignMetadataPayload(formValues, options)
}

export function resolveConditionOptionCode(sources = [], eventType = '', options = {}) {
  const selectedEvent = (options.eventTypes || []).find((e) => e.value === eventType)
  const conditionOptions = selectedEvent?.conditionOptions || []

  const sourcesSet = new Set(sources || [])
  for (const option of conditionOptions) {
    const optSources = option.sources || []
    if (optSources.length === sourcesSet.size && optSources.every((s) => sourcesSet.has(s))) {
      return option.value
    }
  }
  return conditionOptions[0]?.value || ''
}

export function mapCampaignDetailToFormValues(campaign = {}, options = {}) {
  const eventType = campaign.eventType || ''
  const sources = campaign.condition?.sources || []
  const conditionOptionCode = resolveConditionOptionCode(sources, eventType, options)

  return {
    campaignName: campaign.campaignName || '',
    description: campaign.description || '',
    bannerFile: null,
    bannerImageUrl: campaign.bannerImageUrl || '',
    eventType,
    conditionOptionCode,
    startDate: campaign.startDate || '',
    endDate: campaign.endDate || '',
    scheduleCron: campaign.scheduleCron || '',
    durationHour: campaign.durationHour != null ? String(campaign.durationHour) : '',
    userLimitTotal: campaign.userLimitTotal != null ? String(campaign.userLimitTotal) : '',
    userLimitSession: campaign.userLimitSession != null ? String(campaign.userLimitSession) : '',
  }
}

export function mapCampaignActionToFormValues(action = {}) {
  const config = action.actionConfig || {}
  return {
    actionType: action.actionType || 'ISSUE_POINT',
    calculationType: config.calculationType || 'FIXED_AMOUNT',
    recipient: config.recipient || 'EVENT_CUSTOMER',
    amount: config.amount != null ? String(config.amount) : '50',
    executeOrder: action.executeOrder != null ? String(action.executeOrder) : '1',
  }
}
