import { CircleNotchIcon, EyeIcon } from '@phosphor-icons/react'
import { Link } from 'react-router-dom'

import { Button } from '../ui/button'
import {
  formatCustomerVoucherDateRange,
  formatCustomerVoucherDateTime,
  shortenCustomerVoucherId,
} from './customerVoucherFormatters'

function CustomerRedeemsTable({
  items,
  isLoading,
  isRefreshing,
  language,
  t,
  onView,
}) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 inline-flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" aria-hidden="true" />
          {t('common.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[1080px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('voucherRedemptions.columns.customer')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherRedemptions.columns.voucher')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherRedemptions.columns.campaign')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherRedemptions.columns.validity')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherRedemptions.columns.receivedAt')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('voucherRedemptions.columns.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-10 text-center text-muted-foreground" colSpan={6}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" aria-hidden="true" />
                  {t('voucherRedemptions.loading')}
                </span>
              </td>
            </tr>
          ) : (
            items.map((item) => (
              <tr
                key={item.voucherRedemptionId}
                className="border-t border-border transition-colors hover:bg-muted/25"
              >
                <td className="max-w-64 px-4 py-3">
                  {item.cusInfo?.customerId ? (
                    <Link
                      className="block truncate font-semibold text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                      to={`/customer-accounts/${item.cusInfo.customerId}`}
                    >
                      {item.cusInfo.customerUsername || '—'}
                    </Link>
                  ) : (
                    <p className="truncate font-semibold text-foreground">{item.cusInfo?.customerUsername || '—'}</p>
                  )}
                  <p className="truncate text-xs text-muted-foreground">
                    {item.cusInfo?.customerEmail || '—'}
                  </p>
                  {item.cusInfo?.customerPhone ? (
                    <p className="truncate text-xs text-muted-foreground">
                      {item.cusInfo.customerPhone}
                    </p>
                  ) : null}
                </td>
                <td className="max-w-72 px-4 py-3">
                  <p className="truncate font-medium text-foreground">{item.voucherDefName || '—'}</p>
                  <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">
                    {item.voucherDefDescription || '—'}
                  </p>
                </td>
                <td className="max-w-56 px-4 py-3">
                  <p className="truncate text-foreground">{item.campaignName || '—'}</p>
                  {item.campaignId ? (
                    <p className="mt-0.5 font-mono text-xs text-muted-foreground">
                      {shortenCustomerVoucherId(item.campaignId)}
                    </p>
                  ) : null}
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-muted-foreground">
                  {formatCustomerVoucherDateRange(item.validFrom, item.validTo, language)}
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-muted-foreground">
                  {formatCustomerVoucherDateTime(item.redeemAt, language)}
                </td>
                <td className="px-4 py-3 text-right">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onView?.(item.voucherRedemptionId)}
                    aria-label={t('voucherRedemptions.actions.view')}
                  >
                    <EyeIcon aria-hidden="true" />
                    {t('voucherRedemptions.actions.view')}
                  </Button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}

export { CustomerRedeemsTable }
