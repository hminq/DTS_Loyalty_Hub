import { CircleNotchIcon } from '@phosphor-icons/react'
import { Link } from 'react-router-dom'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import {
  formatCampaignDateTime,
  formatCampaignNumber,
  formatCampaignSchedule,
  getCampaignStatusVariant,
} from './campaignFormatters'

function CampaignsTable({
  items,
  options = {},
  isLoading,
  isRefreshing,
  language,
  capabilities = {},
  onView,
  onEdit,
  onDelete,
  t,
}) {
  const hasActions = capabilities.canView || capabilities.canEdit || capabilities.canDelete

  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('common.refreshing', { defaultValue: 'Refreshing...' })}
        </div>
      ) : null}

      <table className="w-full min-w-[860px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="w-72 max-w-72 px-4 py-2.5 font-semibold">{t('campaigns.columns.campaign')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.eventType')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.status')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.schedule')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('campaigns.columns.actionCount')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.updatedAt')}</th>
            {hasActions ? (
              <th className="px-4 py-2.5 text-right font-semibold">{t('common.actions', { defaultValue: 'Actions' })}</th>
            ) : null}
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={hasActions ? 7 : 6}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('campaigns.loading')}
                </span>
              </td>
            </tr>
          ) : (
            items.map((item) => {
              const statusVariant = getCampaignStatusVariant(item.status)

              const statusDef = (options.campaignStatuses || []).find(s => s.value === item.status)
              const statusLabel = statusDef ? statusDef.label : item.status

              const ev = item.eventDefinition || {}
              const eventTypeCode = ev.code || t('campaigns.detail.unknownEvent', { defaultValue: 'Unknown Event' })
              const isDraft = item.status === 'DRAFT'

              return (
                <tr
                  key={item.campaignId}
                  className="border-t border-border transition-colors hover:bg-muted/25"
                >
                  <td className="w-72 max-w-72 px-4 py-3 font-medium text-foreground">
                    <Link
                      to={`/campaigns/${item.campaignId}`}
                      className="block truncate font-semibold text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                      title={item.campaignName}
                    >
                      {item.campaignName}
                    </Link>
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                    {eventTypeCode}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={statusVariant}>
                      {statusLabel}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatCampaignSchedule(item.scheduleCron, item.durationHour, t)}
                  </td>
                  <td className="px-4 py-3 text-right text-muted-foreground">
                    {formatCampaignNumber(item.actionCount, language)}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatCampaignDateTime(item.updatedAt, language)}
                  </td>
                  {hasActions ? (
                    <td className="px-4 py-3 text-right">
                      <div className="flex flex-wrap justify-end gap-1.5">
                        {capabilities.canView ? (
                          <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 px-2.5 text-xs font-medium"
                            onClick={() => onView?.(item.campaignId)}
                          >
                            {t('common.view', { defaultValue: 'View' })}
                          </Button>
                        ) : null}
                        {capabilities.canEdit && isDraft ? (
                          <Button
                            variant="outline"
                            size="sm"
                            className="h-7 px-2.5 text-xs font-medium"
                            onClick={() => onEdit?.(item.campaignId)}
                          >
                            {t('common.edit')}
                          </Button>
                        ) : null}
                        {capabilities.canDelete && isDraft ? (
                          <Button
                            variant="destructive"
                            size="sm"
                            className="h-7 px-2.5 text-xs font-medium"
                            onClick={() => onDelete?.(item)}
                          >
                            {t('common.delete')}
                          </Button>
                        ) : null}
                      </div>
                    </td>
                  ) : null}
                </tr>
              )
            })
          )}
        </tbody>
      </table>
    </div>
  )
}

export { CampaignsTable }
