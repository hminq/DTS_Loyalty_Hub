import { resolveConditionPresetCode } from './campaignPayloads'

export function describeCampaignCondition({ condition, eventType, options, t }) {
  const selectedEvent = (options.eventTypes || []).find((e) => e.value === eventType)
  
  if (!condition) {
    return {
      presetCode: '',
      label: '—',
      predicates: [],
      isMatchAll: false,
      isSupported: false,
    }
  }

  // 1. Check if it's a known preset
  const presetCode = resolveConditionPresetCode(condition, selectedEvent)
  if (presetCode) {
    const presetDef = (selectedEvent?.conditionPresets || []).find(p => p.value === presetCode)
    return {
      presetCode,
      label: presetDef?.label || presetCode,
      predicates: [],
      isMatchAll: presetCode === 'ALL_REGISTRATIONS', // Conventionally
      isSupported: true,
    }
  }

  // 2. Not a preset, it's a raw condition. We only support "all" with EQUALS or IN
  const allPredicates = condition.all || []
  const isMatchAll = Object.keys(condition).length === 1 && allPredicates.length === 0
  
  if (isMatchAll) {
    return {
      presetCode: '',
      label: t('campaigns.conditionPresets.ALL_REGISTRATIONS', { defaultValue: 'All events' }),
      predicates: [],
      isMatchAll: true,
      isSupported: true,
    }
  }

  const isSupported = Array.isArray(condition.all)
  const predicates = []

  if (isSupported) {
    for (const pred of allPredicates) {
      if (pred.fact) { // Simplified matching
        const fieldDef = (selectedEvent?.conditionFields || []).find(f => f.code === pred.fact)
        
        let operatorLabel = pred.operator
        if (pred.operator === 'equal') operatorLabel = t('campaigns.operators.equal', { defaultValue: 'Equals' })
        else if (pred.operator === 'in') operatorLabel = t('campaigns.operators.in', { defaultValue: 'In' })
        
        let value = pred.value
        let valueLabels = []
        
        if (Array.isArray(value)) {
           valueLabels = value.map(v => {
             const opt = (fieldDef?.options || []).find(o => o === v)
             return opt ? opt : v
           })
        } else {
           const opt = (fieldDef?.options || []).find(o => o === value)
           valueLabels = [opt ? opt : value]
        }

        predicates.push({
          fieldCode: pred.fact,
          fieldLabel: fieldDef ? t(`campaigns.conditionFields.${pred.fact}`, { defaultValue: pred.fact }) : pred.fact,
          operator: pred.operator,
          operatorLabel: operatorLabel,
          values: Array.isArray(value) ? value : [value],
          valueLabels: valueLabels
        })
      }
    }
  }

  return {
    presetCode: '',
    label: t('campaigns.conditionPresets.CUSTOM', { defaultValue: 'Custom condition' }),
    predicates,
    isMatchAll,
    isSupported,
  }
}

export function describeCampaignAction({ action, eventType, options, t }) {
  if (!action) {
    return { isSupported: false }
  }

  const config = action.actionConfig || {}
  const targetSelector = config.target?.selector
  const parametersRaw = config.parameters || {}

  const selectedEvent = (options.eventTypes || []).find((e) => e.value === eventType)
  const actionDef = (options.actionTypes || []).find((a) => a.value === action.actionType)
  const targetDef = (selectedEvent?.targets || []).find((t) => t.value === targetSelector)

  const actionTypeCode = action.actionType
  const actionTypeLabel = actionDef 
    ? actionDef.label 
    : actionTypeCode || '—'

  const targetLabel = targetDef
    ? targetDef.label
    : targetSelector || '—'

  const parameters = []
  
  if (actionDef) {
    for (const paramDef of actionDef.parameters || []) {
      const val = parametersRaw[paramDef.code]
      if (val !== undefined) {
        parameters.push({
          code: paramDef.code,
          label: paramDef.label,
          dataType: paramDef.dataType,
          value: val,
          isKnown: true
        })
      }
    }
  }

  // Add unknown parameters
  for (const key of Object.keys(parametersRaw)) {
    if (!parameters.find(p => p.code === key)) {
      parameters.push({
        code: key,
        label: key,
        dataType: 'UNKNOWN',
        value: parametersRaw[key],
        isKnown: false
      })
    }
  }

  const isSupported = Boolean(actionTypeCode && targetSelector)

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
