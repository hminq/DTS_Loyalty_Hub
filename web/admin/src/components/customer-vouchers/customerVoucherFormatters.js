export function formatCustomerVoucherDateTime(value, language) {
  if (!value) return '—'

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'

  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date)
}

export function formatCustomerVoucherDateRange(from, to, language) {
  return `${formatCustomerVoucherDateTime(from, language)} – ${formatCustomerVoucherDateTime(to, language)}`
}

export function shortenCustomerVoucherId(value) {
  return value ? `${value.slice(0, 8)}…${value.slice(-4)}` : '—'
}
