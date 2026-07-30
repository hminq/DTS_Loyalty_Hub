export const FIELD_CODE_REGEX = /^[a-z][A-Za-z0-9]*$/
export const TARGET_SELECTOR_REGEX = /^[A-Z][A-Z0-9_]*$/
export const EVENT_CODE_REGEX = /^[A-Z][A-Z0-9_]*$/
export const ROUTING_KEY_REGEX = /^[a-z0-9]+(?:\.[a-z0-9_-]+)*$/

export function getCompatibleIdFields(fields = []) {
  return fields.filter(
    (field) => field.code && field.type === 'STRING' && field.format === 'UUID' && field.required === true,
  )
}

export function validateMetadataForm(formValues, { canEditIdentity = true } = {}) {
  const errors = {}

  if (canEditIdentity) {
    if (!formValues.code || !formValues.code.trim()) {
      errors.code = 'eventDefinitions.validation.codeRequired'
    } else if (formValues.code.length > 100) {
      errors.code = 'eventDefinitions.validation.codeMaxLength'
    } else if (!EVENT_CODE_REGEX.test(formValues.code.trim())) {
      errors.code = 'eventDefinitions.validation.codeInvalidPattern'
    }

    if (!formValues.routingKey || !formValues.routingKey.trim()) {
      errors.routingKey = 'eventDefinitions.validation.routingKeyRequired'
    } else if (formValues.routingKey.length > 255) {
      errors.routingKey = 'eventDefinitions.validation.routingKeyMaxLength'
    } else if (!ROUTING_KEY_REGEX.test(formValues.routingKey.trim())) {
      errors.routingKey = 'eventDefinitions.validation.routingKeyInvalidPattern'
    }
  }

  if (!formValues.name || !formValues.name.trim()) {
    errors.name = 'eventDefinitions.validation.nameRequired'
  } else if (formValues.name.length > 200) {
    errors.name = 'eventDefinitions.validation.nameMaxLength'
  }

  if (formValues.description && formValues.description.length > 2000) {
    errors.description = 'eventDefinitions.validation.descriptionMaxLength'
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}

export function validateSchemaForm(fields = [], targets = []) {
  const fieldErrors = []
  const targetErrors = []
  const generalErrors = []

  const seenFieldCodes = new Map()
  fields.forEach((field, index) => {
    const errs = {}
    const code = field.code?.trim()

    if (!code) {
      errs.code = 'eventDefinitions.validation.fieldCodeRequired'
    } else if (!FIELD_CODE_REGEX.test(code)) {
      errs.code = 'eventDefinitions.validation.fieldCodeInvalidPattern'
    } else if (seenFieldCodes.has(code)) {
      errs.code = 'eventDefinitions.validation.fieldCodeDuplicate'
    } else {
      seenFieldCodes.set(code, index)
    }

    if (!field.type) {
      errs.type = 'eventDefinitions.validation.fieldTypeRequired'
    }

    if (field.type !== 'STRING' && field.format) {
      errs.format = 'eventDefinitions.validation.fieldFormatNotAllowed'
    }

    fieldErrors[index] = errs
  })

  const compatibleIdFields = getCompatibleIdFields(fields)
  const compatibleIdFieldCodes = new Set(compatibleIdFields.map((f) => f.code?.trim()))

  const seenTargetSelectors = new Map()
  targets.forEach((target, index) => {
    const errs = {}
    const selector = target.selector?.trim()
    const idField = target.idField?.trim()

    if (!selector) {
      errs.selector = 'eventDefinitions.validation.targetSelectorRequired'
    } else if (!TARGET_SELECTOR_REGEX.test(selector)) {
      errs.selector = 'eventDefinitions.validation.targetSelectorInvalidPattern'
    } else if (seenTargetSelectors.has(selector)) {
      errs.selector = 'eventDefinitions.validation.targetSelectorDuplicate'
    } else {
      seenTargetSelectors.set(selector, index)
    }

    if (!target.kind) {
      errs.kind = 'eventDefinitions.validation.targetKindRequired'
    }

    if (!idField) {
      errs.idField = 'eventDefinitions.validation.targetIdFieldRequired'
    } else if (!compatibleIdFieldCodes.has(idField)) {
      errs.idField = 'eventDefinitions.validation.targetIdFieldIncompatible'
    }

    targetErrors[index] = errs
  })

  const hasFieldErrors = fieldErrors.some((err) => Object.keys(err).length > 0)
  const hasTargetErrors = targetErrors.some((err) => Object.keys(err).length > 0)

  return {
    isValid: !hasFieldErrors && !hasTargetErrors && generalErrors.length === 0,
    fieldErrors,
    targetErrors,
    generalErrors,
  }
}
