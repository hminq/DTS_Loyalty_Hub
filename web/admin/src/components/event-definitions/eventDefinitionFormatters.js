import { format, parseISO } from 'date-fns'

export function formatDateTime(isoString) {
  if (!isoString) return '—'
  try {
    return format(parseISO(isoString), 'yyyy-MM-dd HH:mm:ss')
  } catch {
    return isoString
  }
}

export function getStatusBadgeVariant(status) {
  switch (status) {
    case 'ACTIVE':
    case 'PUBLISHED':
      return 'success'
    case 'DRAFT':
    case 'RETIRED':
      return 'secondary'
    default:
      return 'outline'
  }
}

export function formatVersionLabel(version, status) {
  if (!version) return '—'
  return `v${version}` + (status ? ` (${status})` : '')
}
