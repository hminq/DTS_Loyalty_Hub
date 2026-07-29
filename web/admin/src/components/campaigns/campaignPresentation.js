import { describeCondition, inspectPersistedCondition } from './campaignConditions.js'

export function describeCampaignCondition({ condition, eventDefinition, options, t }) {
  const selectedVersion = (options?.eventTypeVersions || []).find(
    (v) => v.value === eventDefinition?.eventTypeVersionId
  )

  if (!condition) {
    return {
      label: '—',
      predicates: [],
      isMatchAll: false,
      isSupported: false,
    }
  }

  const inspection = inspectPersistedCondition(condition, selectedVersion)

  if (!inspection.isSupported) {
    return {
      label: t('campaigns.condition.unsupported', { defaultValue: 'Unsupported configuration' }),
      predicates: [],
      isMatchAll: false,
      isSupported: false,
    }
  }

  if (inspection.isMatchAll) {
    return {
      label: t('campaigns.condition.allValidEvents', { defaultValue: 'All valid events' }),
      predicates: [],
      isMatchAll: true,
      isSupported: true,
    }
  }

  const fieldsMeta = selectedVersion?.conditionFields || selectedVersion?.condition?.fields || []
  const predicates = inspection.predicates.map((p) => {
    const fieldMeta = fieldsMeta.find((f) => f.code === p.field)
    const fieldLabel = fieldMeta ? fieldMeta.label : p.field
    const operatorLabel = t(`campaigns.conditionOperators.${p.operator}`, { defaultValue: p.operator })
    return {
      fieldCode: p.field,
      fieldLabel,
      operator: p.operator,
      operatorLabel,
      values: [p.value],
      valueLabels: [String(p.value)],
    }
  })

  const description = describeCondition({ condition, eventDefinition, selectedVersion, t })

  return {
    label: description,
    predicates,
    isMatchAll: false,
    isSupported: true,
  }
}

export function describeCampaignAction({ action, eventDefinition, options, t }) {
  if (!action || typeof action !== 'object') {
    return { isSupported: false }
  }

  const config = action.actionConfig || {}
  const configKeys = Object.keys(config)
  if (
    typeof config !== 'object' ||
    config === null ||
    Array.isArray(config) ||
    configKeys.length !== 2 ||
    !configKeys.includes('target') ||
    !configKeys.includes('parameters') ||
    !config.target ||
    typeof config.target !== 'object' ||
    config.target === null ||
    Array.isArray(config.target) ||
    Object.keys(config.target).length !== 1 ||
    !Object.keys(config.target).includes('selector') ||
    typeof config.parameters !== 'object' ||
    config.parameters === null ||
    Array.isArray(config.parameters)
  ) {
    return { isSupported: false }
  }

  const targetSelector = config.target?.selector
  const parametersRaw = config.parameters || {}

  const selectedVersion = (options?.eventTypeVersions || []).find(
    (v) => v.value === eventDefinition?.eventTypeVersionId
  )
  const actionDef = (options?.actionTypes || []).find((a) => a.value === action.actionType)

  const availableTargets = selectedVersion?.targets || []
  const targetDef = availableTargets.find(
    (t) => t.value === targetSelector || t.selector === targetSelector
  )

  const actionTypeCode = action.actionType
  const actionTypeLabel = actionDef
    ? actionDef.label
    : actionTypeCode || '—'

  const targetLabel = targetDef
    ? targetDef.label
    : targetSelector || '—'

  let isSupported = Boolean(
    actionDef &&
    targetDef &&
    targetSelector &&
    targetDef.targetKind &&
    actionDef.requiredTargetKind &&
    targetDef.targetKind === actionDef.requiredTargetKind
  )

  const parameters = []

  if (actionDef) {
    for (const paramDef of actionDef.parameters || []) {
      if (paramDef.dataType !== 'DECIMAL') {
        isSupported = false
      }

      const val = parametersRaw[paramDef.code]
      if (val !== undefined && val !== null && val !== '') {
        let isParamValid = true
        if (paramDef.dataType === 'DECIMAL') {
          if (typeof val !== 'number' || !Number.isFinite(val)) {
            isParamValid = false
          }
        }
        if (!isParamValid) {
          isSupported = false
        }
        parameters.push({
          code: paramDef.code,
          label: paramDef.label,
          dataType: paramDef.dataType,
          value: val,
          isKnown: true,
        })
      } else if (paramDef.required) {
        isSupported = false
      }
    }
  } else {
    isSupported = false
  }

  // Add unknown parameters
  for (const key of Object.keys(parametersRaw)) {
    if (!parameters.find((p) => p.code === key)) {
      isSupported = false
      parameters.push({
        code: key,
        label: key,
        dataType: 'UNKNOWN',
        value: parametersRaw[key],
        isKnown: false,
      })
    }
  }

  return {
    actionTypeCode,
    actionTypeLabel,
    targetSelector,
    targetLabel,
    targetKind: targetDef?.targetKind || '',
    parameters,
    executeOrder: action.executeOrder ?? 1,
    totalCount: action.totalCount,
    sessionCount: action.sessionCount,
    isSupported,
  }
}
