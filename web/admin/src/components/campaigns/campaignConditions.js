export function createConditionRow() {
  return {
    rowId: crypto.randomUUID(),
    field: '',
    operator: '',
    value: '',
  }
}

export function createMatchAllConditionFormState() {
  return {
    conditionMode: 'MATCH_ALL',
    conditionPredicates: [],
    originalCondition: null,
  }
}

const UUID_REGEX = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/

function getConditionFields(selectedVersion) {
  return selectedVersion?.conditionFields || selectedVersion?.condition?.fields || []
}

function normalizeConditionValue(fieldMeta, value, fieldCode) {
  if (fieldMeta.dataType === 'STRING') {
    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error(`Invalid string value for field ${fieldCode}`)
    }
    if (fieldMeta.format === 'UUID' && !UUID_REGEX.test(value)) {
      throw new Error(`Invalid UUID value for field ${fieldCode}: ${value}`)
    }
    return value
  }

  if (fieldMeta.dataType === 'NUMBER') {
    const num = Number(value)
    if (!Number.isFinite(num) || String(value).trim() === '') {
      throw new Error(`Invalid number value for field ${fieldCode}: ${value}`)
    }
    return num
  }

  if (fieldMeta.dataType === 'BOOLEAN') {
    if (value === true || value === 'true') return true
    if (value === false || value === 'false') return false
    throw new Error(`Invalid boolean value for field ${fieldCode}: ${value}`)
  }

  throw new Error(`Unsupported field data type: ${fieldMeta.dataType}`)
}

function normalizeConditionPredicate(predicate, fieldsMeta) {
  if (typeof predicate !== 'object' || predicate === null || Array.isArray(predicate)) {
    throw new Error('Invalid condition predicate shape')
  }

  const predKeys = Object.keys(predicate)
  if (
    predKeys.length !== 3 ||
    !predKeys.includes('field') ||
    !predKeys.includes('operator') ||
    !predKeys.includes('value')
  ) {
    throw new Error('Invalid condition predicate properties')
  }

  const { field, operator, value } = predicate
  if (typeof field !== 'string' || !field || typeof operator !== 'string' || !operator) {
    throw new Error('Invalid condition predicate field or operator')
  }

  const fieldMeta = fieldsMeta.find((f) => f.code === field)
  if (!fieldMeta) {
    throw new Error(`Unknown condition field: ${field}`)
  }

  if (!fieldMeta.operators?.includes(operator)) {
    throw new Error(`Unsupported operator ${operator} for field ${field}`)
  }

  return {
    field,
    operator,
    value: normalizeConditionValue(fieldMeta, value, field),
  }
}

function getPredicateKey(predicate) {
  return `${predicate.field}|${predicate.operator}|${JSON.stringify(predicate.value)}`
}

export function inspectPersistedCondition(condition, selectedVersion = null) {
  if (
    typeof condition !== 'object' ||
    condition === null ||
    Array.isArray(condition)
  ) {
    return { isSupported: false, isMatchAll: false, predicates: [], problems: ['INVALID_ROOT'] }
  }

  const keys = Object.keys(condition)
  if (keys.length !== 1 || keys[0] !== 'all' || !Array.isArray(condition.all)) {
    return { isSupported: false, isMatchAll: false, predicates: [], problems: ['INVALID_ROOT_KEYS'] }
  }

  if (condition.all.length === 0) {
    return { isSupported: true, isMatchAll: true, predicates: [], problems: [] }
  }

  if (!selectedVersion) {
    return { isSupported: false, isMatchAll: false, predicates: [], problems: ['MISSING_VERSION_METADATA'] }
  }

  const fieldsMeta = getConditionFields(selectedVersion)
  const predicates = []
  const seenKeys = new Set()
  let isSupported = true
  const problems = []

  for (const pred of condition.all) {
    let normalized
    try {
      normalized = normalizeConditionPredicate(pred, fieldsMeta)
    } catch (error) {
      isSupported = false
      problems.push(error.message)
      continue
    }

    const dupKey = getPredicateKey(normalized)
    if (seenKeys.has(dupKey)) {
      isSupported = false
      problems.push('DUPLICATE_PREDICATE')
    }
    seenKeys.add(dupKey)
    predicates.push(normalized)
  }

  return {
    isSupported,
    isMatchAll: false,
    predicates,
    problems,
  }
}

export function mapConditionToFormState(condition, selectedVersion = null) {
  const inspection = inspectPersistedCondition(condition, selectedVersion)

  if (!inspection.isSupported) {
    return {
      conditionMode: 'UNSUPPORTED',
      conditionPredicates: [],
      originalCondition: condition,
    }
  }

  if (inspection.isMatchAll) {
    return {
      conditionMode: 'MATCH_ALL',
      conditionPredicates: [],
      originalCondition: condition,
    }
  }

  const predicates = inspection.predicates.map((p) => ({
    rowId: crypto.randomUUID(),
    field: p.field,
    operator: p.operator,
    value: String(p.value),
  }))

  return {
    conditionMode: 'MATCH_FIELDS',
    conditionPredicates: predicates,
    originalCondition: condition,
  }
}

export function buildConditionFromFormState(formValues, selectedVersion) {
  if (formValues?.conditionMode === 'UNSUPPORTED') {
    throw new Error('Cannot build payload from UNSUPPORTED condition mode')
  }

  if (!selectedVersion) {
    throw new Error('Selected event version metadata is required')
  }

  if (formValues?.conditionMode === 'MATCH_ALL') {
    return { all: [] }
  }

  if (formValues?.conditionMode !== 'MATCH_FIELDS') {
    throw new Error(`Unknown condition mode: ${formValues?.conditionMode}`)
  }

  const predicates = formValues.conditionPredicates || []
  if (predicates.length === 0) {
    throw new Error('MATCH_FIELDS requires at least one condition row')
  }

  const fieldsMeta = getConditionFields(selectedVersion)
  const seenKeys = new Set()
  const all = []

  for (const predicate of predicates) {
    if (
      !predicate.field ||
      !predicate.operator ||
      predicate.value === '' ||
      predicate.value === undefined ||
      predicate.value === null
    ) {
      throw new Error('Incomplete condition predicate row')
    }

    const normalized = normalizeConditionPredicate(
      {
        field: predicate.field,
        operator: predicate.operator,
        value: predicate.value,
      },
      fieldsMeta,
    )
    const key = getPredicateKey(normalized)
    if (seenKeys.has(key)) {
      throw new Error(`Duplicate condition predicate: ${key}`)
    }
    seenKeys.add(key)
    all.push(normalized)
  }

  return { all }
}

export function validateConditionFormState(formValues, selectedVersion, t) {
  const errors = {}

  if (formValues?.conditionMode === 'UNSUPPORTED') {
    errors.condition = t('campaigns.validation.unsupportedCondition', {
      defaultValue: 'Unsupported condition configuration',
    })
  } else if (formValues?.conditionMode === 'MATCH_FIELDS') {
    const predicates = formValues.conditionPredicates || []
    if (predicates.length === 0) {
      errors.condition = t('campaigns.validation.conditionRequired', {
        defaultValue: 'At least one condition is required',
      })
    } else {
      const rowErrors = []
      const seen = new Set()
      let hasError = false
      const fieldsMeta = getConditionFields(selectedVersion)

      predicates.forEach((predicate, idx) => {
        const rowErr = {}
        if (
          !predicate.field ||
          !predicate.operator ||
          predicate.value === '' ||
          predicate.value === undefined ||
          predicate.value === null
        ) {
          rowErr.incomplete = t('campaigns.validation.incompleteCondition', {
            defaultValue: 'Incomplete condition',
          })
          hasError = true
        } else {
          const fieldMeta = fieldsMeta.find((field) => field.code === predicate.field)
          if (!fieldMeta) {
            rowErr.field = t('campaigns.validation.unsupportedField', {
              defaultValue: 'Unsupported field type',
            })
            hasError = true
          } else if (!fieldMeta.operators?.includes(predicate.operator)) {
            rowErr.operator = t('campaigns.validation.unsupportedOperator', {
              defaultValue: 'Unsupported operator',
            })
            hasError = true
          } else {
            let typedValue
            try {
              typedValue = normalizeConditionValue(fieldMeta, predicate.value, predicate.field)
            } catch {
              if (fieldMeta.dataType === 'NUMBER') {
                rowErr.value = t('campaigns.validation.invalidNumber', {
                  defaultValue: 'Invalid number',
                })
              } else if (fieldMeta.dataType === 'STRING' && fieldMeta.format === 'UUID') {
                rowErr.value = t('campaigns.validation.invalidUuid', {
                  defaultValue: 'Invalid UUID',
                })
              } else if (fieldMeta.dataType === 'BOOLEAN') {
                rowErr.value = t('campaigns.validation.invalidBoolean', {
                  defaultValue: 'Invalid boolean',
                })
              } else {
                rowErr.value = t('campaigns.validation.incompleteCondition', {
                  defaultValue: 'Incomplete condition',
                })
              }
              hasError = true
            }

            if (typedValue !== undefined) {
              const key = getPredicateKey({
                field: predicate.field,
                operator: predicate.operator,
                value: typedValue,
              })
              if (seen.has(key)) {
                rowErr.duplicate = t('campaigns.validation.duplicatePredicate', {
                  defaultValue: 'Duplicate predicate',
                })
                hasError = true
              }
              seen.add(key)
            }
          }
        }
        rowErrors[idx] = Object.keys(rowErr).length > 0 ? rowErr : null
      })

      if (hasError) {
        errors.conditionRows = rowErrors
      }
    }
  }

  return Object.keys(errors).length > 0 ? errors : null
}

export function describeCondition({ condition, eventDefinition, selectedVersion, t }) {
  const inspection = inspectPersistedCondition(condition, selectedVersion)

  if (!inspection.isSupported) {
    return t('campaigns.condition.unsupported', { defaultValue: 'Unsupported configuration' })
  }

  if (inspection.isMatchAll) {
    return t('campaigns.condition.allValidEvents', { defaultValue: 'All valid events' })
  }

  const fieldsMeta = getConditionFields(selectedVersion)
  const lines = inspection.predicates.map((pred) => {
    const fieldMeta = fieldsMeta.find((f) => f.code === pred.field)
    const fieldLabel = fieldMeta ? fieldMeta.label : pred.field
    const opLabel = t(`campaigns.conditionOperators.${pred.operator}`, { defaultValue: pred.operator })
    return `${fieldLabel} ${opLabel} ${pred.value}`
  })

  return lines.join(' AND ')
}
