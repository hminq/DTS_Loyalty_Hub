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

function mapEventTypes(eventTypes = [], t) {
  return (eventTypes || []).map((item) => {
    const code = item?.code || ''

    const condition = item?.condition || {}
    const combinators = condition.combinators || []
    const fields = condition.fields || []
    const presets = (condition.presets || []).map(preset => ({
      value: preset.code,
      label: t(`campaigns.conditionPresets.${preset.code}`, { defaultValue: preset.code }),
      condition: preset.condition,
    }))

    const targets = (item?.targets || []).map(target => ({
      value: target.code,
      label: t(`campaigns.targetSelectors.${target.code}`, { defaultValue: target.code }),
      targetKind: target.targetKind,
      applicability: target.applicability,
    }))

    return {
      value: code,
      label: t(`campaigns.eventTypes.${code}`, { defaultValue: code }),
      conditionCombinators: combinators,
      conditionFields: fields,
      conditionPresets: presets,
      targets: targets,
    }
  })
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
  return {
    campaignStatuses: mapStatuses(opts.campaignStatuses, t),
    eventTypes: mapEventTypes(opts.eventTypes, t),
    actionTypes: mapActionTypes(opts.actionTypes, t),
    schedule: mapSchedule(opts.schedule, t),
  }
}
