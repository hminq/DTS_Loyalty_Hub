import { CircleNotchIcon, EyeIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { formatDateTime, getStatusBadgeVariant } from './eventDefinitionFormatters'

export function EventDefinitionsTable({
  items = [],
  isLoading = false,
  isRefreshing = false,
}) {
  const { t } = useTranslation()

  return (
    <div className="relative overflow-x-auto">
      {isRefreshing && (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('common.refreshing')}
        </div>
      )}

      <table className="w-full min-w-[800px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.nameAndCode')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.routingKey')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.status')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.latestVersion')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.publishedVersion')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.draftVersion')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.updatedAt')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('common.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={8}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('eventDefinitions.loading')}
                </span>
              </td>
            </tr>
          ) : items.length === 0 ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={8}>
                {t('eventDefinitions.emptyTitle')}
              </td>
            </tr>
          ) : (
            items.map((item) => {
              const statusVariant = getStatusBadgeVariant(item.status)
              const statusLabel = t(`eventDefinitions.statuses.${item.status}`, item.status)

              return (
                <tr
                  key={item.eventTypeId}
                  className="border-t border-border transition-colors hover:bg-muted/25"
                >
                  <td className="px-4 py-3">
                    <Link
                      to={`/event-definitions/${item.eventTypeId}`}
                      className="font-medium text-foreground hover:text-primary hover:underline"
                    >
                      <div>{item.name}</div>
                      <div className="font-mono text-xs text-muted-foreground">{item.code}</div>
                    </Link>
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                    {item.routingKey}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={statusVariant}>{statusLabel}</Badge>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {item.latestVersion ? `v${item.latestVersion}` : '—'}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {item.latestPublishedVersion ? `v${item.latestPublishedVersion}` : '—'}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {item.draftVersion ? `v${item.draftVersion}` : '—'}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatDateTime(item.updatedAt)}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Link to={`/event-definitions/${item.eventTypeId}`}>
                      <Button variant="ghost" size="sm" className="h-8 px-2">
                        <EyeIcon data-icon="inline-start" />
                        {t('common.view')}
                      </Button>
                    </Link>
                  </td>
                </tr>
              )
            })
          )}
        </tbody>
      </table>
    </div>
  )
}
