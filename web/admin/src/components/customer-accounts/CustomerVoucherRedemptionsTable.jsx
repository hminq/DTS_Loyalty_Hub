import { CircleNotchIcon } from '@phosphor-icons/react'

import { formatCustomerDateTime } from './customerAccountFormatters'

function CustomerVoucherRedemptionsTable({ redemptions, isLoading, isRefreshing, language, t }) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('customerAccounts.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[800px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.redemptions.columns.voucher')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.redemptions.columns.campaign')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.redemptions.columns.action')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.redemptions.columns.sourceEvent')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.redemptions.columns.redeemedAt')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={5}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('customerAccounts.redemptions.loading')}
                </span>
              </td>
            </tr>
          ) : redemptions.length === 0 ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={5}>
                {t('customerAccounts.redemptions.empty')}
              </td>
            </tr>
          ) : (
            redemptions.map((redemption) => (
              <tr key={redemption.voucherRedemptionId} className="border-t border-border transition-colors hover:bg-muted/25">
                <td className="px-4 py-3">
                  <p className="font-semibold text-foreground">{redemption.voucherDefinitionName || t('customerAccounts.emptyValue')}</p>
                  <p className="mt-0.5 font-mono text-xs text-muted-foreground">
                    {redemption.voucherCode}
                  </p>
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {redemption.campaignName || t('customerAccounts.emptyValue')}
                </td>
                <td className="px-4 py-3 font-medium text-foreground">
                  {redemption.actionType || t('customerAccounts.emptyValue')}
                </td>
                <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                  {redemption.sourceEventId || t('customerAccounts.emptyValue')}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {formatCustomerDateTime(redemption.redeemedAt, language) || '-'}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}

export { CustomerVoucherRedemptionsTable }
