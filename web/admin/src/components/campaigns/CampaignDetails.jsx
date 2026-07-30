import { ArrowUpRightIcon, ImageSquareIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'

import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import {
  formatCampaignDateTime,
  formatCampaignNumber,
  formatCampaignSchedule,
  getCampaignStatusVariant,
} from './campaignFormatters'
import { describeCampaignCondition } from './campaignPresentation'

export function CampaignDetails({ campaign, options = {}, language, t }) {
  const [imageError, setImageError] = useState(false)

  useEffect(() => {
    setImageError(false)
  }, [campaign?.bannerImageUrl])

  if (!campaign) return null

  const statusVariant = getCampaignStatusVariant(campaign.status)
  const statusLabel = t(`campaigns.statuses.${campaign.status}`, {
    defaultValue: campaign.status || '—',
  })

  const eventTypeLabel = campaign.eventDefinition
    ? campaign.eventDefinition.name && campaign.eventDefinition.name !== campaign.eventDefinition.code
      ? `${campaign.eventDefinition.name} - ${campaign.eventDefinition.code} (v${campaign.eventDefinition.version})`
      : `${campaign.eventDefinition.code} (v${campaign.eventDefinition.version})`
    : t('campaigns.detail.unknownEvent', { defaultValue: 'Unknown event' })

  const conditionDesc = describeCampaignCondition({
    condition: campaign.condition,
    eventDefinition: campaign.eventDefinition,
    options,
    t,
  })

  const timeZone = options.schedule?.timeZone || 'UTC'
  const hasBanner = Boolean(campaign.bannerImageUrl) && !imageError

  return (
    <div className="grid gap-5">
      {/* Banner Card */}
      <Card className="overflow-hidden rounded-xl border-border/80 shadow-none">
        <CardContent className="p-0">
          {hasBanner ? (
            <div className="group relative w-full bg-muted/20">
              <img
                src={campaign.bannerImageUrl}
                alt={campaign.campaignName || t('campaigns.form.bannerLabel')}
                className="h-48 w-full object-cover transition-transform duration-300 group-hover:scale-[1.005] sm:h-60 md:h-72"
                onError={() => setImageError(true)}
              />
              <div className="absolute bottom-3 right-3">
                <a
                  href={campaign.bannerImageUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 rounded-md border border-border/80 bg-background/95 px-3 py-1.5 text-xs font-semibold text-foreground shadow-sm backdrop-blur transition-all hover:bg-background hover:shadow-md"
                >
                  {t('campaigns.detail.openBanner', {
                    defaultValue: 'Open image in new tab',
                  })}
                  <ArrowUpRightIcon size={14} aria-hidden="true" />
                </a>
              </div>
            </div>
          ) : (
            <div className="flex h-48 w-full flex-col items-center justify-center gap-2.5 bg-muted/30 text-muted-foreground sm:h-60">
              <ImageSquareIcon size={36} className="opacity-40" aria-hidden="true" />
              <p className="text-xs font-medium">
                {t('campaigns.detail.noBanner', {
                  defaultValue: 'No banner image uploaded for this campaign.',
                })}
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* General information */}
      <Card className="rounded-xl border-border/80 shadow-none">
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <CardTitle>
            {t('campaigns.detail.identityTitle', { defaultValue: 'General information' })}
          </CardTitle>
          <Badge variant={statusVariant}>{statusLabel}</Badge>
        </CardHeader>
        <CardContent className="grid gap-6">
          {/* General Information */}
          <div className="grid gap-4 sm:grid-cols-2">
            <DetailItem
              label={t('campaigns.form.campaignNameLabel', { defaultValue: 'Campaign name' })}
              value={campaign.campaignName}
              prominent
            />
            <DetailItem
              label={t('campaigns.form.descriptionLabel', { defaultValue: 'Description' })}
              value={campaign.description || t('common.none', { defaultValue: 'None' })}
            />
          </div>

          {/* Customer limits */}
          <div className="grid gap-4 border-t border-border pt-4 sm:grid-cols-2">
            <DetailItem
              label={t('campaigns.form.userLimitTotalLabel', {
                defaultValue: 'Max rewards per customer',
              })}
              value={
                campaign.userLimitTotal != null
                  ? formatCampaignNumber(campaign.userLimitTotal, language)
                  : t('campaigns.detail.unlimited', { defaultValue: 'Unlimited' })
              }
            />
            <DetailItem
              label={t('campaigns.form.userLimitSessionLabel', {
                defaultValue: 'Max rewards per customer per session',
              })}
              value={
                campaign.userLimitSession != null
                  ? formatCampaignNumber(campaign.userLimitSession, language)
                  : t('campaigns.detail.unlimited', { defaultValue: 'Unlimited' })
              }
            />
          </div>

          {/* Schedule & Active Range */}
          <div className="grid gap-4 border-t border-border pt-4 sm:grid-cols-3">
            <DetailItem
              label={t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
              value={formatCampaignDateTime(campaign.startDate, language)}
            />
            <DetailItem
              label={t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
              value={formatCampaignDateTime(campaign.endDate, language)}
            />
            <DetailItem
              label={t('campaigns.form.scheduleCronLabel', { defaultValue: 'Schedule CRON' })}
              value={`${formatCampaignSchedule(
                campaign.scheduleCron,
                campaign.durationHour,
                t,
              )} (${timeZone})`}
              mono
            />
          </div>

          {/* Timestamps & ID */}
          <div className="grid gap-4 border-t border-border pt-4 sm:grid-cols-3">
            <DetailItem
              label={t('campaigns.detail.campaignId', { defaultValue: 'Campaign ID' })}
              value={campaign.campaignId}
              mono
            />
            <DetailItem
              label={t('common.createdAt', { defaultValue: 'Created at' })}
              value={formatCampaignDateTime(campaign.createdAt, language)}
              muted
            />
            <DetailItem
              label={t('campaigns.columns.updatedAt', { defaultValue: 'Updated at' })}
              value={formatCampaignDateTime(campaign.updatedAt, language)}
              muted
            />
          </div>
        </CardContent>
      </Card>

      {/* Event and condition */}
      <Card className="rounded-xl border-border/80 shadow-none">
        <CardHeader>
          <CardTitle>
            {t('campaigns.detail.eventConditionTitle', { defaultValue: 'Event & condition' })}
          </CardTitle>
        </CardHeader>
        <CardContent className="grid gap-5">
          <DetailItem
            label={t('campaigns.form.eventTypeLabel', { defaultValue: 'Event type' })}
            value={eventTypeLabel}
          />

          <div className="border-t border-border pt-4">
            <p className="mb-3 text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
              {t('campaigns.form.conditionLabel', { defaultValue: 'Campaign condition' })}
            </p>
            {!conditionDesc.isSupported ? (
              <div className="text-xs font-medium text-amber-600">
                {t('campaigns.detail.unsupportedCondition', { defaultValue: 'Warning: Unsupported condition configuration.' })}
              </div>
            ) : conditionDesc.predicates?.length > 0 ? (
              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {conditionDesc.predicates.map((p, i) => (
                  <div
                    key={i}
                    className="flex flex-wrap items-center gap-2 rounded-lg border border-border/60 bg-muted/20 p-3 text-xs"
                  >
                    <span className="font-semibold text-foreground">{p.fieldLabel}</span>
                    <span className="rounded bg-muted/80 px-2 py-0.5 text-[11px] font-medium text-muted-foreground">
                      {p.operatorLabel}
                    </span>
                    <span className="rounded border border-border/50 bg-background px-2 py-0.5 font-mono text-[11px] font-semibold text-primary">
                      {p.valueLabels.join(', ')}
                    </span>
                  </div>
                ))}
              </div>
            ) : (
              <div className="inline-flex items-center rounded-md bg-muted/40 px-3 py-1.5 text-xs font-medium text-muted-foreground">
                {conditionDesc.label}
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

function DetailItem({
  label,
  value,
  mono = false,
  muted = false,
  prominent = false,
}) {
  const valueClass = [
    'mt-1 break-words',
    prominent ? 'text-base font-semibold text-foreground' : 'text-sm font-medium',
    mono ? 'font-mono text-xs' : '',
    muted ? 'text-muted-foreground' : 'text-foreground',
  ].filter(Boolean).join(' ')

  return (
    <div>
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
        {label}
      </p>
      {typeof value === 'string' || typeof value === 'number' ? (
        <p className={valueClass}>{value || '—'}</p>
      ) : (
        <div className={valueClass}>{value || '—'}</div>
      )}
    </div>
  )
}
