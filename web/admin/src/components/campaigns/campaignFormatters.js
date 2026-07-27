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

export function formatCampaignSchedule(cron, durationHour, t) {
  if (!cron) return '—'
  if (durationHour === null || durationHour === undefined) return cron
  return `${cron} (${durationHour} ${t('campaigns.hours', { count: durationHour })})`
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
