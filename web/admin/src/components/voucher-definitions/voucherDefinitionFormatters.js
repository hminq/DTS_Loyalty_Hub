export function formatVoucherNumber(value, language) {
  if (value === null || value === undefined) return '0'
  return new Intl.NumberFormat(language || 'en').format(value)
}

export function formatVoucherDateTime(value, language) {
  if (!value) return '—'
  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function formatVoucherReward(type, value, language, t) {
  if (type === 'PERCENT') {
    return value !== null && value !== undefined ? `${value}%` : '—'
  }
  if (type === 'FIXED') {
    return value !== null && value !== undefined ? formatVoucherNumber(value, language) : '—'
  }
  if (type === 'GIFT') {
    return t ? t('voucherDefinitions.types.reward.GIFT') : 'Gift'
  }
  return value !== null && value !== undefined ? String(value) : '—'
}

export function getVoucherRecordState(deletedAt) {
  return deletedAt ? 'DELETED' : 'ACTIVE'
}
