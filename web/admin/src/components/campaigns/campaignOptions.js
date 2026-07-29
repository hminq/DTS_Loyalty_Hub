function mapStatuses(statuses = [], t) {
  return (statuses || []).map((code) => ({
    value: code,
    label: t(`campaigns.statuses.${code}`, { defaultValue: code }),
  }))
}

function mapSchedule(schedule = {}, t) {
  return {
    timeZone: schedule?.timeZone || 'UTC',
    daysOfWeek: (schedule?.daysOfWeek || []).map((day) => ({
      value: day,
      label: t(`campaigns.daysOfWeek.${day}`, { defaultValue: day }),
    })),
  }
}

function mapEventTypeVersions(eventTypeVersions = [], t) {
  return (eventTypeVersions || []).map((item) => {
    const eventTypeId = item?.eventTypeId || ''
    const eventTypeVersionId = item?.eventTypeVersionId || ''
    const code = item?.code || ''
    const routingKey = item?.routingKey || ''
    const name = item?.name || ''
    const version = item?.version || 1

    const condition = item?.condition || {}
    const combinators = condition.combinators || []
    const fields = (condition.fields || []).map(field => ({
      code: field.code,
      label: t(`campaigns.conditionFields.${field.code}`, { defaultValue: field.code }),
      dataType: field.dataType,
      format: field.format,
      required: field.required,
      operators: field.operators,
      options: field.options,
    }))

    const targets = (item?.targets || []).map(target => ({
      value: target.selector,
      selector: target.selector,
      label: t(`campaigns.targetSelectors.${target.selector}`, { defaultValue: target.selector }),
      targetKind: target.targetKind,
      idField: target.idField,
    }))

    return {
      value: eventTypeVersionId,
      label: `${name} - ${code} (v${version})`,
      eventTypeId,
      eventTypeVersionId,
      code,
      routingKey,
      name,
      version,
      conditionCombinators: combinators,
      conditionFields: fields,
      targets: targets,
    }
  })
}

function mapEventTypeFilters(eventTypeVersions = []) {
  const uniqueTypes = new Map()

  ;(eventTypeVersions || []).forEach((item) => {
    if (item?.eventTypeId && !uniqueTypes.has(item.eventTypeId)) {
      uniqueTypes.set(item.eventTypeId, {
        value: item.eventTypeId,
        label: `${item.name} (${item.code})`
      })
    }
  })

  return Array.from(uniqueTypes.values())
}

function mapActionTypes(actionTypes = [], t) {
  return (actionTypes || []).map((item) => {
    const code = item?.code || ''
    return {
      value: code,
      label: t(`campaigns.actionTypes.${code}`, { defaultValue: code }),
      requiredTargetKind: item?.requiredTargetKind,
      parameters: (item?.parameters || []).map(param => ({
        code: param.code,
        label: t(`campaigns.parameters.${param.code}`, { defaultValue: param.code }),
        dataType: param.dataType,
        required: param.required,
        minimumExclusive: param.minimumExclusive,
        maximum: param.maximum,
        scale: param.scale,
      })),
    }
  })
}

export function mapCampaignOptions(rawOptions = {}, t) {
  const opts = rawOptions || {}
  const eventTypeVersions = mapEventTypeVersions(opts.eventTypeVersions, t)
  return {
    campaignStatuses: mapStatuses(opts.campaignStatuses, t),
    eventTypeVersions,
    eventTypeFilters: mapEventTypeFilters(opts.eventTypeVersions),
    actionTypes: mapActionTypes(opts.actionTypes, t),
    schedule: mapSchedule(opts.schedule, t),
  }
}
