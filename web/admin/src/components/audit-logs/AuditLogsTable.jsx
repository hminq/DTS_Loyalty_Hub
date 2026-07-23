import { useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { AuditLogDetails, actorName, formatDateTime, shortenId } from './AuditLogDetails'

function AuditLogsTable({ auditLogs, isLoading, language, hasActiveFilters }) {
  const { t } = useTranslation()
  const [expandedId, setExpandedId] = useState(null)

  return (
    <div className="relative overflow-x-auto">
      <table className="w-full min-w-[850px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/70 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <Header>{t('auditLogs.columns.time')}</Header><Header>{t('auditLogs.columns.actor')}</Header>
            <Header>{t('auditLogs.columns.action')}</Header><Header>{t('auditLogs.columns.entity')}</Header>
            <Header>{t('auditLogs.columns.entityId')}</Header><Header className="text-right">{t('auditLogs.columns.actions')}</Header>
          </tr>
        </thead>
        <tbody>
          {auditLogs.map((auditLog) => (
            <AuditRow key={auditLog.auditLogId} auditLog={auditLog} language={language} expanded={expandedId === auditLog.auditLogId} onToggle={() => setExpandedId((current) => current === auditLog.auditLogId ? null : auditLog.auditLogId)} t={t} />
          ))}
        </tbody>
      </table>
    </div>
  )
}

function AuditRow({ auditLog, language, expanded, onToggle, t }) {
  return (
    <>
      <tr className="border-t border-border">
        <Cell className="whitespace-nowrap text-muted-foreground">{formatDateTime(auditLog.createdAt, language)}</Cell>
        <Cell><span className="font-medium">{actorName(auditLog, t)}</span></Cell>
        <Cell><Badge variant="secondary" className="font-mono text-[11px]">{auditLog.action}</Badge></Cell>
        <Cell className="font-medium">{auditLog.entityType}</Cell>
        <Cell className="font-mono text-xs text-muted-foreground" title={auditLog.entityId || undefined}>{shortenId(auditLog.entityId) || '—'}</Cell>
        <Cell className="text-right"><Button variant="ghost" size="sm" onClick={onToggle} aria-expanded={expanded}>{t(expanded ? 'auditLogs.actions.close' : 'auditLogs.actions.view')}</Button></Cell>
      </tr>
      {expanded ? <tr className="border-t border-border"><td colSpan={6}><AuditLogDetails auditLog={auditLog} language={language} /></td></tr> : null}
    </>
  )
}

function Header({ className = '', children }) { return <th className={`px-3 py-2.5 font-semibold ${className}`}>{children}</th> }
function Cell({ className = '', children }) { return <td className={`px-3 py-3 ${className}`}>{children}</td> }
function MessageRow({ children }) { return <tr className="border-t border-border"><td className="px-3 py-5 text-muted-foreground" colSpan={6}>{children}</td></tr> }

export { AuditLogsTable }
