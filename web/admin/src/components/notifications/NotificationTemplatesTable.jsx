import { ArrowRightIcon, CircleNotchIcon } from '@phosphor-icons/react'
import { Badge } from '../ui/badge'
import { Button } from '../ui/button'

function NotificationTemplatesTable({ templates, isLoading, isRefreshing, language, onView, onToggleStatus, t }) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} />
          {t('notifications.refreshing', 'Refreshing...')}
        </div>
      ) : null}

      <table className="w-full min-w-[760px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('notifications.columns.name', 'Template Name')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('notifications.columns.eventType', 'Event Type')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('notifications.columns.channel', 'Channel')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('notifications.columns.status', 'Status')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('notifications.columns.createdAt', 'Created At')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('notifications.columns.actions', 'Actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={6}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} />
                  {t('notifications.loading', 'Loading templates...')}
                </span>
              </td>
            </tr>
          ) : templates.map((template) => (
            <tr key={template.templateId} className="border-t border-border transition-colors hover:bg-muted/25">
              <td className="px-4 py-3">
                <p className="font-semibold text-foreground">{template.name}</p>
                <p className="mt-0.5 text-xs text-muted-foreground">{template.language}</p>
              </td>
              <td className="px-4 py-3 text-muted-foreground">{template.eventTypeDisplayName || template.eventTypeCode}</td>
              <td className="px-4 py-3">
                <Badge variant="outline">{template.channel}</Badge>
              </td>
              <td className="px-4 py-3">
                <Badge variant={template.isActive ? 'success' : 'secondary'}>
                  {template.isActive
                    ? t('notifications.status.active', 'Active')
                    : t('notifications.status.inactive', 'Inactive')}
                </Badge>
              </td>
              <td className="px-4 py-3 text-muted-foreground">
                {formatDateTime(template.createdAt, language)}
              </td>
              <td className="px-4 py-3 text-right">
                <Button variant="ghost" size="sm" onClick={() => onToggleStatus(template.templateId)}>
                  {template.isActive ? t('notifications.actions.deactivate', 'Deactivate') : t('notifications.actions.activate', 'Activate')}
                </Button>
                <Button variant="ghost" size="sm" onClick={() => onView(template.templateId)}>
                  {t('notifications.actions.edit', 'Edit')}
                  <ArrowRightIcon size={14} />
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function formatDateTime(value, language) {
  if (!value) {
    return '-'
  }

  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export { NotificationTemplatesTable }
