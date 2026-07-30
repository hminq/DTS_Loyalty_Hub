import {
  CAMPAIGN_SCHEDULE_MODES,
  parseCampaignScheduleCron,
} from './campaignSchedule.js'

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
  const schedule = parseCampaignScheduleCron(cron)

  if (schedule.mode === CAMPAIGN_SCHEDULE_MODES.DAILY) {
    return t
      ? t('campaigns.scheduleDaily', {
          time: schedule.time,
          defaultValue: `Daily at ${schedule.time}`,
        })
      : `Daily at ${schedule.time}`
  }

  if (schedule.mode === CAMPAIGN_SCHEDULE_MODES.WEEKLY) {
    const days = schedule.weekdays.join(',')
    return t
      ? t('campaigns.scheduleWeekly', {
          days,
          time: schedule.time,
          defaultValue: `Every ${days} at ${schedule.time}`,
        })
      : `Every ${days} at ${schedule.time}`
  }

  if (schedule.mode === CAMPAIGN_SCHEDULE_MODES.MONTHLY) {
    const days = schedule.daysOfMonth.join(', ')
    return t
      ? t('campaigns.scheduleMonthly', {
          days,
          time: schedule.time,
          defaultValue: `Days ${days} of every month at ${schedule.time}`,
        })
      : `Days ${days} of every month at ${schedule.time}`
  }

  if (schedule.mode === CAMPAIGN_SCHEDULE_MODES.LAST_DAY) {
    return t
      ? t('campaigns.scheduleLastDay', {
          time: schedule.time,
          defaultValue: `Last day of every month at ${schedule.time}`,
        })
      : `Last day of every month at ${schedule.time}`
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
