import { CircleNotchIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { formatCustomerDateTime, formatCustomerNumber } from './customerAccountFormatters'

function CustomerPointTransactionsTable({ transactions, isLoading, isRefreshing, language, t }) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('customerAccounts.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[840px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.transactions.columns.type')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.transactions.columns.amount')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.transactions.columns.balance')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.transactions.columns.campaignAction')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.transactions.columns.sourceEvent')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.transactions.columns.createdAt')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={6}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('customerAccounts.transactions.loading')}
                </span>
              </td>
            </tr>
          ) : transactions.length === 0 ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={6}>
                {t('customerAccounts.transactions.empty')}
              </td>
            </tr>
          ) : (
            transactions.map((tx) => (
              <tr key={tx.pointTransactionId} className="border-t border-border transition-colors hover:bg-muted/25">
                <td className="px-4 py-3 font-medium text-foreground">
                  <Badge variant="outline" className="font-mono text-xs">
                    {tx.transactionType || t('customerAccounts.emptyValue')}
                  </Badge>
                </td>
                <td className="px-4 py-3 font-semibold text-foreground">
                  {formatCustomerNumber(tx.amount, language)}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {formatCustomerNumber(tx.balanceBefore, language)} → {formatCustomerNumber(tx.balanceAfter, language)}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  <p className="font-medium text-foreground">{tx.campaignName || '-'}</p>
                  <p className="text-xs text-muted-foreground">{tx.actionType || '-'}</p>
                </td>
                <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                  {tx.sourceEventId || '-'}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {formatCustomerDateTime(tx.createdAt, language) || '-'}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}

export { CustomerPointTransactionsTable }
