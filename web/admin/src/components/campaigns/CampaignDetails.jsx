import { ImageSquareIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import {
  formatCampaignDateTime,
  formatCampaignNumber,
  formatCampaignSchedule,
  getCampaignStatusVariant,
} from './campaignFormatters'
import { resolveConditionOptionCode } from './campaignPayloads'

export function CampaignDetails({ campaign, options = {}, language, t }) {
  if (!campaign) return null

  const statusVariant = getCampaignStatusVariant(campaign.status)
  const statusLabel = t(`campaigns.statuses.${campaign.status}`, {
    defaultValue: campaign.status || '—',
  })

  const eventTypeLabel = t(`campaigns.eventTypes.${campaign.eventType}`, {
    defaultValue: campaign.eventType || '—',
  })

  const conditionCode = resolveConditionOptionCode(
    campaign.condition?.sources,
    campaign.eventType,
    options,
  )
  const conditionLabel = conditionCode
    ? t(`campaigns.conditionOptions.${conditionCode}`, { defaultValue: conditionCode })
    : (campaign.condition?.sources || []).join(', ') || '—'

  const timeZone = options.schedule?.timeZone || 'UTC'

  return (
    <div className="grid gap-6">
      {/* Identity & Banner */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <CardTitle>
            {t('campaigns.detail.identityTitle', { defaultValue: 'General information' })}
          </CardTitle>
          <Badge variant={statusVariant}>{statusLabel}</Badge>
        </CardHeader>
        <CardContent className="grid gap-6 sm:grid-cols-2">
          <div className="space-y-4">
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.campaignNameLabel', { defaultValue: 'Campaign name' })}
              </p>
              <p className="mt-1 text-base font-semibold text-foreground">
                {campaign.campaignName || '—'}
              </p>
            </div>

            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.detail.campaignId', { defaultValue: 'Campaign ID' })}
              </p>
              <p className="mt-1 font-mono text-xs text-muted-foreground">
                {campaign.campaignId || '—'}
              </p>
            </div>

            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {t('campaigns.form.descriptionLabel', { defaultValue: 'Description' })}
              </p>
              <p className="mt-1 text-sm text-foreground">
                {campaign.description || t('common.none', { defaultValue: 'None' })}
              </p>
            </div>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground mb-2">
              {t('campaigns.form.bannerLabel', { defaultValue: 'Campaign banner' })}
            </p>
            {campaign.bannerImageUrl ? (
              <div className="overflow-hidden rounded-lg border bg-muted/30">
                <img
                  src={campaign.bannerImageUrl}
                  alt={campaign.campaignName || 'Campaign banner'}
                  className="h-[180px] w-full object-cover"
                />
              </div>
            ) : (
              <div className="flex h-[180px] flex-col items-center justify-center gap-2 rounded-lg border border-dashed bg-muted/10 text-muted-foreground">
                <ImageSquareIcon size={32} className="opacity-50" />
                <p className="text-xs">
                  {t('campaigns.detail.noBanner', { defaultValue: 'No banner image uploaded' })}
                </p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Event & Condition */}
      <Card>
        <CardHeader>
          <CardTitle>
            {t('campaigns.detail.eventConditionTitle', { defaultValue: 'Event & condition' })}
          </CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.eventTypeLabel', { defaultValue: 'Event type' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">{eventTypeLabel}</p>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.conditionSourceLabel', { defaultValue: 'Campaign condition' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">{conditionLabel}</p>
          </div>
        </CardContent>
      </Card>

      {/* Active Range & Schedule */}
      <Card>
        <CardHeader>
          <CardTitle>
            {t('campaigns.form.scheduleTitle', { defaultValue: 'Schedule & active range' })}
          </CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-3">
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">
              {formatCampaignDateTime(campaign.startDate, language)}
            </p>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">
              {formatCampaignDateTime(campaign.endDate, language)}
            </p>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.scheduleCronLabel', { defaultValue: 'Schedule CRON' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">
              {formatCampaignSchedule(campaign.scheduleCron, campaign.durationHour, t)} ({timeZone})
            </p>
          </div>
        </CardContent>
      </Card>

      {/* Limits & Timestamps */}
      <Card>
        <CardHeader>
          <CardTitle>
            {t('campaigns.detail.limitsTimestampsTitle', {
              defaultValue: 'Limits & timestamps',
            })}
          </CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-4">
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.userLimitTotalLabel', { defaultValue: 'Total user limit' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">
              {campaign.userLimitTotal != null
                ? formatCampaignNumber(campaign.userLimitTotal, language)
                : t('common.unlimited', { defaultValue: 'Unlimited' })}
            </p>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.userLimitSessionLabel', { defaultValue: 'Session user limit' })}
            </p>
            <p className="mt-1 text-sm font-medium text-foreground">
              {campaign.userLimitSession != null
                ? formatCampaignNumber(campaign.userLimitSession, language)
                : t('common.unlimited', { defaultValue: 'Unlimited' })}
            </p>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('common.createdAt', { defaultValue: 'Created at' })}
            </p>
            <p className="mt-1 text-sm font-medium text-muted-foreground">
              {formatCampaignDateTime(campaign.createdAt, language)}
            </p>
          </div>

          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t('campaigns.columns.updatedAt', { defaultValue: 'Updated at' })}
            </p>
            <p className="mt-1 text-sm font-medium text-muted-foreground">
              {formatCampaignDateTime(campaign.updatedAt, language)}
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
