import { CircleNotchIcon } from '@phosphor-icons/react'
import { Link } from 'react-router-dom'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { shortenCustomerVoucherId } from './customerVoucherFormatters'

function CustomerVouchersTable({
  items,
  isLoading,
  isRefreshing,
  onView,
  t,
}) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 inline-flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" aria-hidden="true" />
          {t('common.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[840px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('customerVouchers.columns.customer')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerVouchers.columns.voucher')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerVouchers.columns.rewardType')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerVouchers.columns.status')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('customerVouchers.columns.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-10 text-center text-muted-foreground" colSpan={5}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" aria-hidden="true" />
                  {t('customerVouchers.loading')}
                </span>
              </td>
            </tr>
          ) : (
            items.map((item) => (
              <tr
                key={item.cusVoucherId}
                className="border-t border-border transition-colors hover:bg-muted/25"
              >
                <td className="max-w-72 px-4 py-3">
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
                <td className="max-w-80 px-4 py-3">
                  <p className="truncate font-medium text-foreground">{item.voucherDefName || '—'}</p>
                  <p className="mt-0.5 font-mono text-xs text-muted-foreground">
                    {shortenCustomerVoucherId(item.cusVoucherId)}
                  </p>
                </td>
                <td className="px-4 py-3">
                  <Badge variant="outline">
                    {t(`customerVouchers.rewardTypes.${item.voucherDefRewardType}`, {
                      defaultValue: item.voucherDefRewardType || '—',
                    })}
                  </Badge>
                </td>
                <td className="px-4 py-3">
                  <Badge variant={item.isExpired ? 'destructive' : 'success'}>
                    {t(`customerVouchers.status.${item.isExpired ? 'expired' : 'available'}`)}
                  </Badge>
                </td>
                <td className="px-4 py-3 text-right">
                  <Button variant="outline" size="sm" onClick={() => onView(item)}>
                    {t('customerVouchers.actions.view')}
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

export { CustomerVouchersTable }
