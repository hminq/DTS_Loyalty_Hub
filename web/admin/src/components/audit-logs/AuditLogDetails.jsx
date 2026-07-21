import { useTranslation } from 'react-i18next'

function AuditLogDetails({ auditLog, language }) {
  const { t } = useTranslation()

  return (
    <div className="grid gap-4 bg-muted/20 p-4 lg:grid-cols-2">
      <Detail label={t('auditLogs.details.auditLogId')} value={auditLog.auditLogId} mono />
      <Detail label={t('auditLogs.details.createdAt')} value={formatDateTime(auditLog.createdAt, language)} />
      <Detail label={t('auditLogs.details.actor')} value={actorName(auditLog, t)} />
      <Detail label={t('auditLogs.details.actorUserId')} value={auditLog.actorUserId} mono />
      <Detail label={t('auditLogs.details.entityType')} value={auditLog.entityType} />
      <Detail label={t('auditLogs.details.entityId')} value={auditLog.entityId} mono />
      <JsonDetail label={t('auditLogs.details.oldValue')} value={auditLog.oldValue} />
      <JsonDetail label={t('auditLogs.details.newValue')} value={auditLog.newValue} />
    </div>
  )
}

function Detail({ label, value, mono = false }) {
  return (
    <div className="min-w-0">
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <p className={`mt-1 break-all text-[13px] ${mono ? 'font-mono' : ''}`}>{value || '—'}</p>
    </div>
  )
}

function JsonDetail({ label, value }) {
  return (
    <div className="min-w-0">
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <pre className="mt-1 max-h-64 overflow-auto rounded-md border border-border bg-background p-3 text-xs leading-5">{formatJson(value)}</pre>
    </div>
  )
}

function formatJson(value) {
  if (!value) return '—'
  try { return JSON.stringify(JSON.parse(value), null, 2) } catch { return value }
}

function actorName(auditLog, t) {
  return auditLog.actorFullName || auditLog.actorUsername || shortenId(auditLog.actorUserId) || t('auditLogs.systemActor')
}

function shortenId(value) {
  return value ? `${value.slice(0, 8)}…${value.slice(-4)}` : ''
}

function formatDateTime(value, language) {
  if (!value) return '—'
  return new Intl.DateTimeFormat(language || 'en', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
}

export { AuditLogDetails, actorName, formatDateTime, shortenId }
