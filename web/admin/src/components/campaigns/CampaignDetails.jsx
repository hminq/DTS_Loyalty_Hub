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
    ? `${campaign.eventDefinition.name} (${campaign.eventDefinition.code}) · v${campaign.eventDefinition.version}`
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

      <div className="grid gap-5 lg:grid-cols-2">
        <Card className="rounded-xl border-border/80 shadow-none lg:row-span-2">
          <CardHeader className="flex flex-row items-center justify-between space-y-0">
            <CardTitle>
              {t('campaigns.detail.identityTitle', { defaultValue: 'General information' })}
            </CardTitle>
            <Badge variant={statusVariant}>{statusLabel}</Badge>
          </CardHeader>
          <CardContent className="grid gap-5">
            <DetailItem
              label={t('campaigns.form.campaignNameLabel', { defaultValue: 'Campaign name' })}
              value={campaign.campaignName}
              prominent
            />
            <DetailItem
              label={t('campaigns.form.descriptionLabel', { defaultValue: 'Description' })}
              value={campaign.description || t('common.none', { defaultValue: 'None' })}
            />
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

        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader>
            <CardTitle>
              {t('campaigns.detail.eventConditionTitle', { defaultValue: 'Event & eligibility' })}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid gap-5 sm:grid-cols-2">
            <DetailItem
              label={t('campaigns.form.eventTypeLabel', { defaultValue: 'Event type' })}
              value={eventTypeLabel}
            />
            <DetailItem
              label={t('campaigns.form.conditionLabel', { defaultValue: 'Campaign condition' })}
              value={
                <div className="space-y-1">
                  <div>{conditionDesc.label}</div>
                  {!conditionDesc.isSupported && (
                    <div className="text-xs text-amber-600 font-medium">
                      {t('campaigns.detail.unsupportedCondition', { defaultValue: 'Warning: Unsupported condition configuration.' })}
                    </div>
                  )}
                  {conditionDesc.predicates?.length > 0 && (
                    <ul className="mt-2 space-y-1.5">
                      {conditionDesc.predicates.map((p, i) => (
                        <li key={i} className="flex flex-wrap items-center gap-1.5 text-xs">
                          <span className="font-semibold text-foreground">{p.fieldLabel}</span>
                          <span className="text-muted-foreground">{p.operatorLabel}</span>
                          <span className="rounded bg-muted/50 px-1.5 py-0.5 font-mono text-[11px] font-medium text-foreground">
                            {p.valueLabels.join(', ')}
                          </span>
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              }
            />
            <DetailItem
              label={t('campaigns.form.userLimitTotalLabel', {
                defaultValue: 'Max rewards per customer',
              })}
              value={
                campaign.userLimitTotal != null
                  ? formatCampaignNumber(campaign.userLimitTotal, language)
                  : t('common.unlimited', { defaultValue: 'Unlimited' })
              }
            />
            <DetailItem
              label={t('campaigns.form.userLimitSessionLabel', {
                defaultValue: 'Max rewards per customer per session',
              })}
              value={
                campaign.userLimitSession != null
                  ? formatCampaignNumber(campaign.userLimitSession, language)
                  : t('common.unlimited', { defaultValue: 'Unlimited' })
              }
            />
          </CardContent>
        </Card>

        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader>
            <CardTitle>
              {t('campaigns.form.scheduleTitle', { defaultValue: 'Schedule & active range' })}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid gap-5 sm:grid-cols-2">
            <DetailItem
              label={t('campaigns.form.startDateLabel', { defaultValue: 'Start date' })}
              value={formatCampaignDateTime(campaign.startDate, language)}
            />
            <DetailItem
              label={t('campaigns.form.endDateLabel', { defaultValue: 'End date' })}
              value={formatCampaignDateTime(campaign.endDate, language)}
            />
            <div className="sm:col-span-2">
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
          </CardContent>
        </Card>
      </div>
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
