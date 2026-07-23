import { ArrowUpRightIcon, TicketIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import {
  formatVoucherDateTime,
  formatVoucherNumber,
  formatVoucherReward,
  getVoucherRecordState,
} from './voucherDefinitionFormatters'

function VoucherDefinitionDetails({ voucher, language, t }) {
  const [imageError, setImageError] = useState(false)
  const isDeleted = getVoucherRecordState(voucher.deletedAt) === 'DELETED'
  const hasImage = Boolean(voucher.bannerImageUrl) && !imageError

  return (
    <div className="mt-5 flex flex-col gap-5">
      {isDeleted ? (
        <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-4 py-3 text-[13px] text-destructive">
          <p className="font-semibold">{t('voucherDefinitions.detail.deletedNotice')}</p>
          <p className="mt-0.5 text-xs opacity-90">
            {t('voucherDefinitions.detail.deletedAtLabel')}: {formatVoucherDateTime(voucher.deletedAt, language)}
          </p>
        </div>
      ) : null}

      {/* 1. Full-width Banner Image Section (FIRST) */}
      <Card className="rounded-xl border-border/80 shadow-none overflow-hidden">
        <CardContent className="p-0">
          {hasImage ? (
            <div className="relative group w-full bg-muted/20">
              <img
                src={voucher.bannerImageUrl}
                alt={voucher.name}
                className="h-56 sm:h-72 md:h-80 w-full object-cover transition-transform duration-300 group-hover:scale-[1.005]"
                onError={() => setImageError(true)}
              />
              <div className="absolute bottom-3 right-3">
                <a
                  href={voucher.bannerImageUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 rounded-md border border-border/80 bg-background/95 px-3 py-1.5 text-xs font-semibold text-foreground shadow-sm backdrop-blur transition-all hover:bg-background hover:shadow-md"
                >
                  {t('voucherDefinitions.detail.openImageInNewTab')}
                  <ArrowUpRightIcon size={14} />
                </a>
              </div>
            </div>
          ) : (
            <div className="flex h-48 sm:h-60 w-full flex-col items-center justify-center gap-2.5 bg-muted/30 text-muted-foreground">
              <TicketIcon size={36} className="opacity-40" />
              <p className="text-xs font-medium">{t('voucherDefinitions.detail.noBannerImage')}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* 2. Details Grid */}
      <div className="grid gap-5 lg:grid-cols-2">
        {/* Left Column */}
        <div className="flex flex-col gap-5">
          {/* General Info Card */}
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm">{t('voucherDefinitions.detail.generalTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 px-4 pb-4 text-[13px]">
              <DetailItem label={t('voucherDefinitions.detail.voucherDefinitionId')} value={voucher.voucherDefinitionId} code />
              <DetailItem label={t('voucherDefinitions.detail.name')} value={voucher.name} />
              <DetailItem
                label={t('voucherDefinitions.detail.code')}
                value={
                  voucher.code ? (
                    <div className="mt-1">
                      <span className="inline-flex items-center rounded-md border border-border/80 bg-muted/50 px-2.5 py-1 font-mono text-[13px] font-bold text-foreground tracking-[0.1em] shadow-sm">
                        {voucher.code}
                      </span>
                    </div>
                  ) : (
                    '—'
                  )
                }
              />
              <div className="min-w-0">
                <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
                  {t('voucherDefinitions.detail.description')}
                </p>
                <div className="mt-1 max-h-40 overflow-y-auto pr-2 break-words font-medium">
                  {voucher.description || '—'}
                </div>
              </div>
            </CardContent>
          </Card>

          {/* System Metadata Card */}
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm">{t('voucherDefinitions.detail.metadataTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2">
              <DetailItem
                label={t('voucherDefinitions.detail.recordState')}
                value={
                  isDeleted ? (
                    <Badge variant="destructive" className="text-xs">{t('voucherDefinitions.recordState.DELETED')}</Badge>
                  ) : (
                    <Badge variant="success" className="text-xs">{t('voucherDefinitions.recordState.ACTIVE')}</Badge>
                  )
                }
              />
              <DetailItem
                label={t('voucherDefinitions.detail.publishType')}
                value={t(`voucherDefinitions.types.publish.${voucher.publishType}`, { defaultValue: voucher.publishType })}
              />
              <DetailItem
                label={t('voucherDefinitions.detail.generationType')}
                value={t(`voucherDefinitions.types.generation.${voucher.generationType}`, { defaultValue: voucher.generationType })}
              />
              <div className="col-span-1" />
              
              <div className="sm:col-span-2 mt-1 grid gap-4 border-t border-border/50 pt-4 sm:grid-cols-2">
                <DetailItem label={t('voucherDefinitions.detail.createdAt')} value={formatVoucherDateTime(voucher.createdAt, language)} />
                <DetailItem label={t('voucherDefinitions.detail.deletedAt')} value={formatVoucherDateTime(voucher.deletedAt, language)} />
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right Column */}
        <div className="flex flex-col gap-5">
          {/* Reward Rules Card */}
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm">{t('voucherDefinitions.detail.rewardTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2">
              <DetailItem
                label={t('voucherDefinitions.detail.rewardType')}
                value={t(`voucherDefinitions.types.reward.${voucher.rewardType}`, { defaultValue: voucher.rewardType })}
              />
              <DetailItem
                label={t('voucherDefinitions.detail.rewardValue')}
                value={voucher.rewardType === 'GIFT' ? '—' : formatVoucherReward(voucher.rewardType, voucher.rewardValue, language, t)}
              />
            </CardContent>
          </Card>

          {/* Inventory Card */}
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm">{t('voucherDefinitions.detail.inventoryTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2">
              <DetailItem label={t('voucherDefinitions.detail.remainingStock')} value={formatVoucherNumber(voucher.remainingStock, language)} />
              <DetailItem label={t('voucherDefinitions.detail.totalStock')} value={formatVoucherNumber(voucher.totalStock, language)} />
            </CardContent>
          </Card>

          {/* Validity Section */}
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm">{t('voucherDefinitions.detail.validityTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2">
              <DetailItem
                label={t('voucherDefinitions.detail.validityType')}
                value={t(`voucherDefinitions.types.validity.${voucher.validityType}`, { defaultValue: voucher.validityType })}
              />
              <DetailItem
                label={t('voucherDefinitions.detail.durationDay')}
                value={voucher.durationDay ? `${voucher.durationDay} ${t('voucherDefinitions.detail.days')}` : '—'}
              />
              <div className="sm:col-span-2 mt-1 grid gap-4 border-t border-border/50 pt-4 sm:grid-cols-2">
                <DetailItem label={t('voucherDefinitions.detail.validFrom')} value={formatVoucherDateTime(voucher.validFrom, language)} />
                <DetailItem label={t('voucherDefinitions.detail.validTo')} value={formatVoucherDateTime(voucher.validTo, language)} />
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

function DetailItem({ label, value, code = false, className = '' }) {
  return (
    <div className={`min-w-0 ${className}`}>
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <div className={code ? 'mt-1 break-all font-mono text-xs' : 'mt-1 break-words font-medium'}>
        {value}
      </div>
    </div>
  )
}

export { VoucherDefinitionDetails }
