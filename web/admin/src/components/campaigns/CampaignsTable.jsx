import { CircleNotchIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import {
  formatCampaignDateTime,
  formatCampaignNumber,
  formatCampaignSchedule,
  getCampaignStatusVariant,
} from './campaignFormatters'

function CampaignsTable({
  items,
  isLoading,
  isRefreshing,
  language,
  t,
}) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('common.refreshing', { defaultValue: 'Refreshing...' })}
        </div>
      ) : null}

      <table className="w-full min-w-[960px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.campaign')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.eventType')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.status')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.activeRange')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.schedule')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('campaigns.columns.actionCount')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.nextSession')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('campaigns.columns.updatedAt')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={8}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('campaigns.loading')}
                </span>
              </td>
            </tr>
          ) : (
            items.map((item) => {
              const statusVariant = getCampaignStatusVariant(item.status)
              const statusLabel = t(`campaigns.statuses.${item.status}`, { defaultValue: item.status })
              const eventTypeLabel = t(`campaigns.eventTypes.${item.eventType}`, { defaultValue: item.eventType })

              return (
                <tr
                  key={item.campaignId}
                  className="border-t border-border transition-colors hover:bg-muted/25"
                >
                  <td className="px-4 py-3 font-medium text-foreground">
                    {item.campaignName}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {eventTypeLabel}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={statusVariant}>
                      {statusLabel}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    <div>{formatCampaignDateTime(item.startDate, language)}</div>
                    <div className="text-xs opacity-80">{formatCampaignDateTime(item.endDate, language)}</div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatCampaignSchedule(item.scheduleCron, item.durationHour, t)}
                  </td>
                  <td className="px-4 py-3 text-right text-muted-foreground">
                    {formatCampaignNumber(item.actionCount, language)}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatCampaignDateTime(item.nextSessionStart, language)}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatCampaignDateTime(item.updatedAt, language)}
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

export { CampaignsTable }
