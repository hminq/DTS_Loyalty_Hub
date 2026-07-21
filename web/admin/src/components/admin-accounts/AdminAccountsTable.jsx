import { ArrowRightIcon, CircleNotchIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'

function AdminAccountsTable({ accounts, isLoading, isRefreshing, language, onView, t }) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} />
          {t('adminAccounts.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[760px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('adminAccounts.columns.account')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('adminAccounts.columns.role')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('adminAccounts.columns.status')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('adminAccounts.columns.createdAt')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('adminAccounts.columns.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={5}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} />
                  {t('adminAccounts.loading')}
                </span>
              </td>
            </tr>
          ) : accounts.map((account) => (
            <tr key={account.adminId} className="border-t border-border transition-colors hover:bg-muted/25">
              <td className="px-4 py-3">
                <p className="font-semibold text-foreground">{account.fullName || account.username}</p>
                <p className="mt-0.5 text-xs text-muted-foreground">@{account.username}</p>
              </td>
              <td className="px-4 py-3 text-muted-foreground">{account.roleName}</td>
              <td className="px-4 py-3">
                <Badge variant={account.status === 'ENABLE' ? 'success' : 'secondary'}>
                  {account.status === 'ENABLE'
                    ? t('adminAccounts.filters.enabled')
                    : t('adminAccounts.filters.disabled')}
                </Badge>
              </td>
              <td className="px-4 py-3 text-muted-foreground">
                {formatDateTime(account.createdAt, language)}
              </td>
              <td className="px-4 py-3 text-right">
                <Button variant="ghost" size="sm" onClick={() => onView(account.adminId)}>
                  {t('adminAccounts.actions.view')}
                  <ArrowRightIcon size={14} />
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function formatDateTime(value, language) {
  if (!value) {
    return '-'
  }

  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export { AdminAccountsTable, formatDateTime }
