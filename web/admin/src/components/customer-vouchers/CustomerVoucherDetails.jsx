import { TicketIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'

import { formatCustomerVoucherDateTime } from './customerVoucherFormatters'

function CustomerVoucherDetails({ voucher, language, t }) {
  const [imageFailed, setImageFailed] = useState(false)
  const hasBanner = voucher.voucherDefBannerImgUrl && !imageFailed

  useEffect(() => {
    setImageFailed(false)
  }, [voucher.cusVoucherId])

  return (
    <div className="mt-5 grid gap-5">
      <div className="overflow-hidden rounded-xl border border-border bg-muted/30">
        {hasBanner ? (
          <img
            className="aspect-[16/5] w-full object-cover"
            src={voucher.voucherDefBannerImgUrl}
            alt={voucher.voucherDefName}
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
          {t('customerVouchers.detail.voucherSection')}
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <DetailItem label={t('customerVouchers.detail.rewardType')} value={t(`customerVouchers.rewardTypes.${voucher.voucherDefRewardType}`, { defaultValue: voucher.voucherDefRewardType })} />
          <DetailItem label={t('customerVouchers.detail.generationType')} value={t(`customerVouchers.generationTypes.${voucher.voucherDefGenerationType}`, { defaultValue: voucher.voucherDefGenerationType })} />
          <DetailItem label={t('customerVouchers.detail.validFrom')} value={formatCustomerVoucherDateTime(voucher.validFrom, language)} />
          <DetailItem label={t('customerVouchers.detail.validTo')} value={formatCustomerVoucherDateTime(voucher.validTo, language)} />
          <DetailItem label={t('customerVouchers.detail.receivedAt')} value={formatCustomerVoucherDateTime(voucher.redeemAt, language)} />
          <DetailItem label={t('customerVouchers.detail.voucherDefinitionId')} value={voucher.voucherDefId} mono />
          <DetailItem label={t('customerVouchers.detail.customerVoucherId')} value={voucher.cusVoucherId} mono wide />
        </div>
      </section>

      <section className="rounded-xl border border-border bg-card p-4">
        <h2 className="text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
          {t('customerVouchers.detail.customerSection')}
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <DetailItem label={t('customerVouchers.detail.username')} value={voucher.cusInfo?.customerUsername} />
          <DetailItem label={t('customerVouchers.detail.email')} value={voucher.cusInfo?.customerEmail} />
          <DetailItem label={t('customerVouchers.detail.phone')} value={voucher.cusInfo?.customerPhone} />
          <DetailItem label={t('customerVouchers.detail.customerId')} value={voucher.cusInfo?.customerId} mono />
        </div>
      </section>
    </div>
  )
}

function DetailItem({ label, value, mono = false, wide = false }) {
  return (
    <div className={wide ? 'min-w-0 sm:col-span-2' : 'min-w-0'}>
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <p className={`mt-1 break-all text-[13px] ${mono ? 'font-mono text-xs' : ''}`}>{value || '—'}</p>
    </div>
  )
}

export { CustomerVoucherDetails }
