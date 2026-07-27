export function formatCampaignNumber(value, language) {
  if (value === null || value === undefined) return '0'
  return new Intl.NumberFormat(language || 'en').format(value)
}

export function formatCampaignDateTime(value, language) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date)
}

function describeQuartzCron(cron, t) {
  if (!cron) return ''
  const parts = cron.trim().split(/\s+/)
  if (parts.length !== 6) return cron

  const [, minute, hour, dayOfMonth, month, dayOfWeek] = parts
  const mm = minute.padStart(2, '0')
  const hh = hour.padStart(2, '0')
  const timeStr = `${hh}:${mm}`

  if (dayOfMonth === '*' && month === '*' && dayOfWeek === '?') {
    return t ? t('campaigns.scheduleDaily', { time: timeStr, defaultValue: `Daily at ${timeStr}` }) : `Daily at ${timeStr}`
  }

  if (dayOfMonth === '?' && month === '*' && dayOfWeek !== '*') {
    return t ? t('campaigns.scheduleWeekly', { days: dayOfWeek, time: timeStr, defaultValue: `Every ${dayOfWeek} at ${timeStr}` }) : `Every ${dayOfWeek} at ${timeStr}`
  }

  return cron
}

export function formatCampaignSchedule(cron, durationHour, t) {
  if (!cron) return '—'
  const readable = describeQuartzCron(cron, t)
  if (durationHour === null || durationHour === undefined) return readable || cron
  return `${readable || cron} (${durationHour} ${t('campaigns.hours', { count: durationHour })})`
}

export function getCampaignStatusVariant(status) {
  switch (status) {
    case 'ACTIVE':
      return 'success'
    case 'DRAFT':
    case 'ENDED':
      return 'secondary'
    case 'CANCELLED':
      return 'destructive'
    default:
      return 'outline'
  }
}
