function mapSupportedItem(item, category, t) {
  const code = typeof item === 'string' ? item : (item?.code || '')
  const supported = typeof item === 'string' ? true : Boolean(item?.supported)
  const baseLabel = t(`campaigns.${category}.${code}`, { defaultValue: code })
  const label = supported ? baseLabel : `${baseLabel} — ${t('campaigns.comingSoon')}`

  return {
    value: code,
    label,
    supported,
    disabled: !supported,
  }
}

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
    return {
      value: code,
      label: t(`campaigns.eventTypes.${code}`, { defaultValue: code }),
      conditionOptions: (item?.condition?.options || []).map((option) => ({
        ...mapSupportedItem(option, 'conditionOptions', t),
        sources: Array.isArray(option?.sources) ? option.sources : [],
      })),
      actionTypes: item?.actionTypes || [],
    }
  })
}

function mapActionTypes(actionTypes = [], t) {
  return (actionTypes || []).map((item) => {
    const code = item?.code || ''
    return {
      value: code,
      label: t(`campaigns.actionTypes.${code}`, { defaultValue: code }),
      calculationTypes: (item?.calculationTypes || []).map((calc) => mapSupportedItem(calc, 'calculationTypes', t)),
      recipients: (item?.recipients || []).map((rec) => mapSupportedItem(rec, 'recipients', t)),
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
