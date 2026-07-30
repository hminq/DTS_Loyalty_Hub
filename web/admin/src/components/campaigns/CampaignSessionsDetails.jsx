import { CalendarBlankIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatCampaignDateTime, getCampaignStatusVariant } from './campaignFormatters'

export function CampaignSessionsDetails({ sessions = [], sessionCount, language, t }) {
  const items = sessions || []

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle>
          {t('campaigns.detail.sessionsTitle', { defaultValue: 'Campaign sessions' })}
        </CardTitle>
        <span className="text-xs font-medium text-muted-foreground">
          {t('campaigns.detail.totalSessions', {
            count: sessionCount ?? items.length,
            defaultValue: `Total: ${sessionCount ?? items.length}`,
          })}
        </span>
      </CardHeader>
      <CardContent>
        {items.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-2 rounded-lg border border-dashed py-12 text-muted-foreground">
            <CalendarBlankIcon size={32} className="opacity-50" />
            <p className="text-sm font-medium">
              {t('campaigns.detail.noSessions', {
                defaultValue: 'No sessions have been generated for this campaign yet.',
              })}
            </p>
          </div>
        ) : (
          <div className="relative overflow-x-auto">
            <table className="w-full border-collapse text-left text-xs">
              <thead className="bg-muted/50 text-[11px] uppercase tracking-wider text-muted-foreground">
                <tr>
                  <th className="px-4 py-2 font-semibold">
                    {t('campaigns.detail.sessionNumber', { defaultValue: 'Session #' })}
                  </th>
                  <th className="px-4 py-2 font-semibold">
                    {t('campaigns.columns.status', { defaultValue: 'Status' })}
                  </th>
                  <th className="px-4 py-2 font-semibold">
                    {t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
                  </th>
                  <th className="px-4 py-2 font-semibold">
                    {t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
                  </th>
                  <th className="px-4 py-2 font-semibold">
                    {t('campaigns.detail.endedAt', { defaultValue: 'Ended at' })}
                  </th>
                </tr>
              </thead>
              <tbody>
                {items.map((session, index) => {
                  const statusVariant = getCampaignStatusVariant(session.status)
                  const statusLabel = t(`campaigns.statuses.${session.status}`, {
                    defaultValue: session.status || '—',
                  })

                  return (
                    <tr
                      key={session.campaignSessionId || session.sessionId || index}
                      className="border-t border-border transition-colors hover:bg-muted/20"
                    >
                      <td className="px-4 py-2.5 font-semibold text-foreground">
                        {session.sessionNumber || session.sessionIndex || index + 1}
                      </td>
                      <td className="px-4 py-2.5">
                        <Badge variant={statusVariant}>{statusLabel}</Badge>
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {formatCampaignDateTime(session.startDate, language)}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {formatCampaignDateTime(session.endDate, language)}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {formatCampaignDateTime(session.endedAt, language)}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
