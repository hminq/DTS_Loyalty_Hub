import { CalendarBlankIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatCampaignDateTime, getCampaignStatusVariant } from './campaignFormatters'

export function CampaignSessionsDetails({ sessions = [], sessionCount, language, t }) {
  const items = sessions || []

  return (
    <Card className="rounded-xl border-border/80 shadow-none">
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-3.5 pt-5 px-6">
        <CardTitle className="text-base font-semibold">
          {t('campaigns.detail.sessionsTitle', { defaultValue: 'Campaign sessions' })}
        </CardTitle>
        <span className="text-xs font-medium text-muted-foreground">
          {t('campaigns.detail.totalSessions', {
            count: sessionCount ?? items.length,
            defaultValue: `Total: ${sessionCount ?? items.length}`,
          })}
        </span>
      </CardHeader>
      <CardContent className="p-0">
        {items.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-2 border-t border-dashed border-border py-12 text-muted-foreground">
            <CalendarBlankIcon size={32} className="opacity-50" />
            <p className="text-sm font-medium">
              {t('campaigns.detail.noSessions', {
                defaultValue: 'No sessions have been generated for this campaign yet.',
              })}
            </p>
          </div>
        ) : (
          <div className="relative overflow-x-auto">
            <table className="w-full min-w-[700px] border-collapse text-left text-[13px]">
              <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-semibold">
                    {t('campaigns.detail.sessionNumber', { defaultValue: 'Session #' })}
                  </th>
                  <th className="px-4 py-2.5 font-semibold">
                    {t('campaigns.columns.status', { defaultValue: 'Status' })}
                  </th>
                  <th className="px-4 py-2.5 font-semibold">
                    {t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
                  </th>
                  <th className="px-4 py-2.5 font-semibold">
                    {t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
                  </th>
                  <th className="px-4 py-2.5 font-semibold">
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
                  const startDate = session.sessionStart || session.startDate
                  const endDate = session.sessionEnd || session.endDate

                  return (
                    <tr
                      key={session.campaignSessionId || session.sessionId || index}
                      className="border-t border-border transition-colors hover:bg-muted/25"
                    >
                      <td className="px-4 py-3 font-semibold text-foreground">
                        {session.sessionNumber || session.sessionIndex || index + 1}
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant={statusVariant}>{statusLabel}</Badge>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatCampaignDateTime(startDate, language)}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatCampaignDateTime(endDate, language)}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
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
