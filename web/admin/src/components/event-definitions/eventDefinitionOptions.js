export const DEFAULT_EVENT_DEFINITION_OPTIONS = Object.freeze({
  fieldTypes: [
    { code: 'STRING', formats: ['UUID'], operators: ['EQUALS', 'NOT_EQUALS', 'CONTAINS'] },
    { code: 'NUMBER', formats: [], operators: ['EQUALS', 'NOT_EQUALS', 'GT', 'GTE', 'LT', 'LTE'] },
    { code: 'BOOLEAN', formats: [], operators: ['EQUALS', 'NOT_EQUALS'] },
  ],
  targetKinds: [
    { code: 'CUSTOMER', identityFieldType: 'STRING', identityFieldFormat: 'UUID' },
  ],
  eventTypeStatuses: ['ACTIVE', 'RETIRED'],
  versionStatuses: ['DRAFT', 'PUBLISHED', 'RETIRED'],
  limits: {
    maximumFields: 100,
    maximumTargets: 20,
    maximumSchemaBytes: 65536,
  },
})

export function mapEventDefinitionOptions(backendOptions = {}, t) {
  const fieldTypes = backendOptions.fieldTypes || DEFAULT_EVENT_DEFINITION_OPTIONS.fieldTypes
  const targetKinds = backendOptions.targetKinds || DEFAULT_EVENT_DEFINITION_OPTIONS.targetKinds
  const eventTypeStatuses = backendOptions.eventTypeStatuses || DEFAULT_EVENT_DEFINITION_OPTIONS.eventTypeStatuses
  const versionStatuses = backendOptions.versionStatuses || DEFAULT_EVENT_DEFINITION_OPTIONS.versionStatuses
  const limits = backendOptions.limits || DEFAULT_EVENT_DEFINITION_OPTIONS.limits

  const statusOptions = eventTypeStatuses.map((status) => ({
    value: status,
    label: t(`eventDefinitions.statuses.${status}`, status),
  }))

  const versionStatusOptions = versionStatuses.map((status) => ({
    value: status,
    label: t(`eventDefinitions.versionStatuses.${status}`, status),
  }))

  const fieldTypeOptions = fieldTypes.map((ft) => ({
    value: ft.code,
    label: t(`eventDefinitions.fieldTypes.${ft.code}`, ft.code),
    formats: ft.formats || [],
    operators: ft.operators || [],
  }))

  const targetKindOptions = targetKinds.map((tk) => ({
    value: tk.code,
    label: t(`eventDefinitions.targetKinds.${tk.code}`, tk.code),
    identityFieldType: tk.identityFieldType,
    identityFieldFormat: tk.identityFieldFormat,
  }))

  return {
    raw: backendOptions,
    fieldTypes,
    targetKinds,
    eventTypeStatuses,
    versionStatuses,
    limits,
    statusOptions,
    versionStatusOptions,
    fieldTypeOptions,
    targetKindOptions,
  }
}
