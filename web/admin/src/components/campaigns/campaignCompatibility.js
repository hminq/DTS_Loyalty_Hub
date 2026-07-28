export function doConditionsOverlap(conditionA, conditionB) {
  if (!isSupportedCondition(conditionA) || !isSupportedCondition(conditionB)) {
    return false
  }

  // An empty all-array is a valid unconstrained condition.
  if (conditionA.all.length === 0 || conditionB.all.length === 0) return true

  const predsA = groupPredicatesByField(conditionA.all)
  const predsB = groupPredicatesByField(conditionB.all)
  if (!predsA || !predsB) return false

  for (const field of Object.keys(predsA)) {
    if (predsB[field]) {
      const valuesA = getAllowedValues(predsA[field])
      const valuesB = getAllowedValues(predsB[field])

      if (!valuesA || !valuesB) return false

      const hasSharedValue = [...valuesA].some((value) => valuesB.has(value))
      if (!hasSharedValue) return false
    }
  }

  return true
}

function isSupportedCondition(condition) {
  if (!condition || typeof condition !== 'object' || !Array.isArray(condition.all)) {
    return false
  }

  return Object.keys(condition).length === 1
}

function groupPredicatesByField(predicates) {
  const grouped = {}

  for (const predicate of predicates) {
    if (
      !predicate ||
      typeof predicate !== 'object' ||
      typeof predicate.field !== 'string' ||
      !predicate.field ||
      typeof predicate.operator !== 'string' ||
      !Object.hasOwn(predicate, 'value') ||
      !isSupportedPredicate(predicate)
    ) {
      return null
    }

    if (!grouped[predicate.field]) grouped[predicate.field] = []
    grouped[predicate.field].push(predicate)
  }

  return grouped
}

function isSupportedPredicate(predicate) {
  if (predicate.operator === 'EQUALS') {
    return typeof predicate.value === 'string' && Boolean(predicate.value)
  }

  if (predicate.operator === 'IN') {
    return (
      Array.isArray(predicate.value) &&
      predicate.value.length > 0 &&
      predicate.value.every((value) => typeof value === 'string' && Boolean(value))
    )
  }

  return false
}

function getAllowedValues(fieldPredicates) {
  let allowed = null

  for (const pred of fieldPredicates) {
    if (pred.operator === 'EQUALS') {
      if (typeof pred.value !== 'string' || !pred.value) return null
      const values = new Set([pred.value])
      allowed = allowed
        ? new Set([...allowed].filter((value) => values.has(value)))
        : values
    } else if (pred.operator === 'IN') {
      if (
        !Array.isArray(pred.value) ||
        pred.value.length === 0 ||
        pred.value.some((value) => typeof value !== 'string' || !value)
      ) {
        return null
      }
      const values = new Set(pred.value)
      allowed = allowed
        ? new Set([...allowed].filter((value) => values.has(value)))
        : values
    } else {
      return null
    }
  }

  return allowed
}

export function isTargetCompatible(target, actionType, conditionPreset) {
  if (!target || !actionType || !conditionPreset) return false

  if (target.targetKind !== actionType.requiredTargetKind) {
    return false
  }

  return doConditionsOverlap(target.applicability, conditionPreset.condition)
}

export function getCompatibleActionTypes(actionTypes = [], event, conditionPreset) {
  if (!event || !conditionPreset) return []

  return actionTypes.filter((actionType) =>
    (event.targets || []).some((target) =>
      isTargetCompatible(target, actionType, conditionPreset),
    ),
  )
}
