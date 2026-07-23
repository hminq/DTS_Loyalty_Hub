export function formatCustomerDateTime(value, language) {
  if (!value) return null

  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function formatCustomerNumber(value, language, fallback = '0') {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return fallback
  }

  return new Intl.NumberFormat(language || 'en', {
    maximumFractionDigits: 2,
  }).format(Number(value))
}
