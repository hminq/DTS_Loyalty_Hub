import { CircleNotchIcon } from '@phosphor-icons/react'

import { formatCustomerDateTime, formatCustomerNumber } from './customerAccountFormatters'

function CustomerVouchersTable({ vouchers, isLoading, isRefreshing, language, t }) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('customerAccounts.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[760px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.vouchers.columns.voucher')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.vouchers.columns.validity')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.vouchers.columns.remaining')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.vouchers.columns.receivedAt')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={4}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('customerAccounts.vouchers.loading')}
                </span>
              </td>
            </tr>
          ) : vouchers.length === 0 ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={4}>
                {t('customerAccounts.vouchers.empty')}
              </td>
            </tr>
          ) : (
            vouchers.map((voucher) => (
              <tr key={voucher.customerVoucherId} className="border-t border-border transition-colors hover:bg-muted/25">
                <td className="px-4 py-3">
                  <p className="font-semibold text-foreground">{voucher.voucherDefinitionName || t('customerAccounts.emptyValue')}</p>
                  <p className="mt-0.5 font-mono text-xs text-muted-foreground">
                    {voucher.voucherCode}
                  </p>
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  <span className="block">{formatCustomerDateTime(voucher.validFrom, language) || '-'}</span>
                  <span className="block text-xs opacity-75">→ {formatCustomerDateTime(voucher.validTo, language) || '-'}</span>
                </td>
                <td className="px-4 py-3 font-medium text-foreground">
                  {formatCustomerNumber(voucher.remainingCount, language)}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {formatCustomerDateTime(voucher.receivedAt, language) || '-'}
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
