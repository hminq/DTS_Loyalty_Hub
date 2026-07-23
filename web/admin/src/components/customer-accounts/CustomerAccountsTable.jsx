import { CircleNotchIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { formatCustomerDateTime } from './customerAccountFormatters'

function CustomerAccountsTable({
  accounts,
  isLoading,
  isRefreshing,
  language,
  canEdit,
  canUpdateStatus,
  onView,
  onEdit,
  onStatusChange,
  t,
}) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('customerAccounts.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[820px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.columns.account')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.columns.tier')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.columns.status')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('customerAccounts.columns.createdAt')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('customerAccounts.columns.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={5}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('customerAccounts.loading')}
                </span>
              </td>
            </tr>
          ) : accounts.map((account) => (
            <tr key={account.customerId} className="border-t border-border transition-colors hover:bg-muted/25">
              <td className="px-4 py-3">
                <p className="font-semibold text-foreground">{account.fullName || account.username}</p>
                <p className="mt-0.5 text-xs text-muted-foreground">
                  @{account.username} · {account.email}
                </p>
              </td>
              <td className="px-4 py-3 text-muted-foreground">
                {account.tierName || t('customerAccounts.unassignedTier')}
              </td>
              <td className="px-4 py-3">
                <Badge variant={account.status === 'ENABLE' ? 'success' : 'secondary'}>
                  {account.status === 'ENABLE'
                    ? t('customerAccounts.status.enabled')
                    : t('customerAccounts.status.disabled')}
                </Badge>
              </td>
              <td className="px-4 py-3 text-muted-foreground">
                {formatCustomerDateTime(account.createdAt, language) || '-'}
              </td>
              <td className="px-4 py-3">
                <div className="flex items-center justify-end gap-2 whitespace-nowrap">
                  <Button variant="ghost" size="sm" onClick={() => onView(account.customerId)}>
                    {t('customerAccounts.actions.view')}
                  </Button>
                  {canEdit ? (
                    <Button variant="outline" size="sm" onClick={() => onEdit(account.customerId)}>
                      {t('customerAccounts.actions.edit')}
                    </Button>
                  ) : null}
                  {canUpdateStatus ? (
                    <Button
                      variant={account.status === 'ENABLE' ? 'destructive' : 'default'}
                      size="sm"
                      onClick={() => onStatusChange(account)}
                    >
                      {t(account.status === 'ENABLE'
                        ? 'customerAccounts.actions.disable'
                        : 'customerAccounts.actions.enable')}
                    </Button>
                  ) : null}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export { CustomerAccountsTable, formatCustomerDateTime }
