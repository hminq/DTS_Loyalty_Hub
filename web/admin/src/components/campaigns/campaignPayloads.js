import { buildConditionFromFormState, mapConditionToFormState } from './campaignConditions.js'

function toNullableInteger(value) {
  if (value === '' || value == null) return null
  return Number(value)
}

function parseParameters(actionValues, actionType, options) {
  const parameters = {}
  const selectedAction = (options.actionTypes || []).find(
    (option) => option.value === actionType,
  )
  if (!selectedAction) {
    throw new Error('Campaign action type is not registered.')
  }

  for (const paramDef of selectedAction.parameters || []) {
    const value = actionValues.parameters?.[paramDef.code]
    if (value === '' || value == null) continue

    if (paramDef.dataType === 'DECIMAL') {
      const parsedValue = Number(value)
      if (!Number.isFinite(parsedValue)) {
        throw new Error(`Campaign action parameter '${paramDef.code}' is invalid.`)
      }
      parameters[paramDef.code] = parsedValue
    } else {
      throw new Error(`Campaign action parameter type '${paramDef.dataType}' is not supported.`)
    }
  }

  return parameters
}

export function buildCampaignActionPayload(actionValues = {}, executeOrder = 1, options = {}) {
  const actionType = actionValues.actionType || null
  const targetSelector = actionValues.targetSelector || null

  return {
    actionType,
    actionConfig: {
      target: {
        selector: targetSelector,
      },
      parameters: parseParameters(actionValues, actionType, options),
    },
    executeOrder,
    totalCount: toNullableInteger(actionValues.totalCount),
    sessionCount: toNullableInteger(actionValues.sessionCount),
  }
}

function buildCampaignMetadataPayload(formValues = {}, options = {}) {
  const campaignName = formValues.campaignName?.trim() || null
  const description = formValues.description?.trim() || null
  // The write contract persists the S3 object key in the legacy bannerImageUrl field.
  // Presigned bannerImageUrl values returned by reads must never be written back.
  const bannerImageUrl = formValues.bannerImageKey || null
  const eventTypeVersionId = formValues.eventTypeVersionId || null
  const scheduleCron = formValues.scheduleCron?.trim() || null

  const selectedVersion = (options.eventTypeVersions || []).find(
    (option) => option.value === eventTypeVersionId,
  )
  if (!selectedVersion) {
    throw new Error('Campaign event version is not selected or unknown.')
  }

  const condition = buildConditionFromFormState(formValues, selectedVersion)

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
    eventTypeVersionId,
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
    buildCampaignActionPayload(action, index + 1, options),
  )

  return {
    ...metadata,
    actions,
  }
}

export function buildCampaignUpdatePayload(formValues = {}, options = {}) {
  return buildCampaignMetadataPayload(formValues, options)
}

export function mapCampaignDetailToFormValues(campaign = {}, options = {}) {
  const eventTypeVersionId = campaign.eventDefinition?.eventTypeVersionId || ''

  const selectedVersion = (options.eventTypeVersions || []).find(
    (option) => option.value === eventTypeVersionId,
  )
  const conditionState = mapConditionToFormState(campaign.condition, selectedVersion)

  return {
    campaignName: campaign.campaignName || '',
    description: campaign.description || '',
    bannerFile: null,
    bannerImageKey: campaign.bannerImageKey || '',
    bannerImageUrl: campaign.bannerImageUrl || '',
    eventTypeVersionId,
    ...conditionState,
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
  const target = config.target || {}
  const parameters = config.parameters || {}

  // Convert API scalar values back to string for the form
  const stringParams = {}
  for (const key of Object.keys(parameters)) {
    stringParams[key] = parameters[key] != null ? String(parameters[key]) : ''
  }

  return {
    actionType: action.actionType || '',
    targetSelector: target.selector || '',
    parameters: stringParams,
    executeOrder: action.executeOrder != null ? String(action.executeOrder) : '1',
    totalCount: action.totalCount == null ? '' : String(action.totalCount),
    sessionCount: action.sessionCount == null ? '' : String(action.sessionCount),
  }
}
