import { TicketIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

import { formatCustomerVoucherDateTime } from './customerVoucherFormatters'

function CustomerRedeemDetails({ redemption, language, t }) {
  const [imageFailed, setImageFailed] = useState(false)
  const { customer, voucher, issuanceSource } = redemption
  const hasBanner = voucher?.bannerImageUrl && !imageFailed

  useEffect(() => {
    setImageFailed(false)
  }, [redemption.voucherRedemptionId])

  return (
    <div className="mt-5 grid gap-5">
      <div className="overflow-hidden rounded-xl border border-border bg-muted/30">
        {hasBanner ? (
          <img
            className="aspect-[16/5] w-full object-cover"
            src={voucher.bannerImageUrl}
            alt={voucher.name}
            onError={() => setImageFailed(true)}
          />
        ) : (
          <div className="grid aspect-[16/5] place-items-center text-muted-foreground">
            <TicketIcon size={30} aria-hidden="true" />
          </div>
        )}
      </div>

      <section className="rounded-xl border border-border bg-card p-4">
        <h2 className="text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
          {t('voucherRedemptions.detail.customerSection')}
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <DetailItem
            label={t('voucherRedemptions.detail.username')}
            value={customer?.username}
            renderValue={customer?.customerId ? (
              <Link
                className="font-semibold text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                to={`/customer-accounts/${customer.customerId}`}
              >
                {customer.username || '-'}
              </Link>
            ) : null}
          />
          <DetailItem label={t('voucherRedemptions.detail.email')} value={customer?.email} />
          <DetailItem label={t('voucherRedemptions.detail.phone')} value={customer?.phone} />
          <DetailItem label={t('voucherRedemptions.detail.receivedAt')} value={formatCustomerVoucherDateTime(redemption.redeemedAt, language)} />
        </div>
      </section>

      <section className="rounded-xl border border-border bg-card p-4">
        <h2 className="text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
          {t('voucherRedemptions.detail.voucherSection')}
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <DetailItem label={t('voucherRedemptions.detail.voucherName')} value={voucher?.name} />
          <DetailItem label={t('voucherRedemptions.detail.voucherCode')} value={voucher?.voucherCode} mono />
          <DetailItem label={t('voucherRedemptions.detail.rewardType')} value={t(`customerVouchers.rewardTypes.${voucher?.rewardType}`, { defaultValue: voucher?.rewardType || '-' })} />
          <DetailItem label={t('voucherRedemptions.detail.rewardValue')} value={formatRewardValue(voucher?.rewardValue)} />
          <DetailItem label={t('voucherRedemptions.detail.generationType')} value={t(`customerVouchers.generationTypes.${voucher?.generationType}`, { defaultValue: voucher?.generationType || '-' })} />
          <DetailItem label={t('voucherRedemptions.detail.validFrom')} value={formatCustomerVoucherDateTime(voucher?.validFrom, language)} />
          <DetailItem label={t('voucherRedemptions.detail.validTo')} value={formatCustomerVoucherDateTime(voucher?.validTo, language)} />
        </div>
      </section>

      <section className="rounded-xl border border-border bg-card p-4">
        <h2 className="text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
          {t('voucherRedemptions.detail.issuanceSourceSection')}
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <DetailItem label={t('voucherRedemptions.detail.campaignName')} value={issuanceSource?.campaignName} />
          <DetailItem label={t('voucherRedemptions.detail.campaignEventType')} value={issuanceSource?.campaignEventType} />
          <DetailItem label={t('voucherRedemptions.detail.sessionStart')} value={formatCustomerVoucherDateTime(issuanceSource?.sessionStart, language)} />
          <DetailItem label={t('voucherRedemptions.detail.sessionEnd')} value={formatCustomerVoucherDateTime(issuanceSource?.sessionEnd, language)} />
          <DetailItem label={t('voucherRedemptions.detail.sessionStatus')} value={issuanceSource?.sessionStatus} />
          <DetailItem label={t('voucherRedemptions.detail.actionType')} value={issuanceSource?.actionType} />
        </div>
      </section>

      <section className="rounded-xl border border-border bg-card p-4">
        <h2 className="text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
          {t('voucherRedemptions.detail.referenceIdsSection')}
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <DetailItem label={t('voucherRedemptions.detail.voucherRedemptionId')} value={redemption.voucherRedemptionId} mono />
          <DetailItem label={t('voucherRedemptions.detail.customerVoucherId')} value={voucher?.customerVoucherId} mono />
          <DetailItem label={t('voucherRedemptions.detail.voucherDefinitionId')} value={voucher?.voucherDefinitionId} mono />
          <DetailItem label={t('voucherRedemptions.detail.voucherPoolId')} value={voucher?.voucherPoolId} mono />
          <DetailItem label={t('voucherRedemptions.detail.customerId')} value={customer?.customerId} mono />
          <DetailItem label={t('voucherRedemptions.detail.campaignId')} value={issuanceSource?.campaignId} mono />
          <DetailItem label={t('voucherRedemptions.detail.campaignSessionId')} value={issuanceSource?.campaignSessionId} mono />
          <DetailItem label={t('voucherRedemptions.detail.actionId')} value={issuanceSource?.actionId} mono />
        </div>
      </section>
    </div>
  )
}

function DetailItem({ label, value, renderValue = null, mono = false }) {
  return (
    <div className="min-w-0">
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <div className={`mt-1 break-all text-[13px] ${mono ? 'font-mono text-xs' : ''}`}>
        {renderValue || value || '-'}
      </div>
    </div>
  )
}

function formatRewardValue(value) {
  if (value === null || value === undefined || value === '') return '-'
  return String(value)
}

export { CustomerRedeemDetails }
