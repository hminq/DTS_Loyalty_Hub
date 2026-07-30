export function buildCanonicalPayloadSchema(fields = [], targets = []) {
  return {
    fields: fields.map((f) => ({
      code: f.code?.trim() || '',
      type: f.type || 'STRING',
      format: f.type === 'STRING' ? (f.format || null) : null,
      required: Boolean(f.required),
      conditionable: Boolean(f.conditionable),
    })),
    targets: targets.map((t) => ({
      selector: t.selector?.trim() || '',
      kind: t.kind || 'CUSTOMER',
      idField: t.idField?.trim() || '',
    })),
  }
}

export function buildCreateEventDefinitionPayload(metadataValues, fields = [], targets = []) {
  return {
    code: metadataValues.code?.trim() || '',
    routingKey: metadataValues.routingKey?.trim() || '',
    name: metadataValues.name?.trim() || '',
    description: metadataValues.description?.trim() || null,
    payloadSchema: buildCanonicalPayloadSchema(fields, targets),
  }
}

export function buildUpdateMetadataPayload(metadataValues) {
  return {
    code: metadataValues.code?.trim() || '',
    routingKey: metadataValues.routingKey?.trim() || '',
    name: metadataValues.name?.trim() || '',
    description: metadataValues.description?.trim() || null,
  }
}

export function buildUpdateDraftSchemaPayload(fields = [], targets = []) {
  return {
    payloadSchema: buildCanonicalPayloadSchema(fields, targets),
  }
}

export function mapVersionToFormState(versionDetail) {
  const schema = versionDetail?.payloadSchema || {}
  const fields = (schema.fields || []).map((f) => ({
    id: f.code || Math.random().toString(36).substring(2, 9),
    code: f.code || '',
    type: f.type || 'STRING',
    format: f.format || '',
    required: Boolean(f.required),
    conditionable: Boolean(f.conditionable),
  }))

  const targets = (schema.targets || []).map((t) => ({
    id: t.selector || Math.random().toString(36).substring(2, 9),
    selector: t.selector || '',
    kind: t.kind || 'CUSTOMER',
    idField: t.idField || '',
  }))

  return { fields, targets }
}

export function mapDetailToMetadataForm(detail) {
  return {
    code: detail?.code || '',
    routingKey: detail?.routingKey || '',
    name: detail?.name || '',
    description: detail?.description || '',
  }
}
