import { MagnifyingGlassIcon } from '@phosphor-icons/react'

import { Input } from '../ui/input'

function AdminAccountsFilters({
  keyword,
  onKeywordChange,
  status,
  onStatusChange,
  roleId,
  onRoleChange,
  roleOptions,
  isLoadingRoles,
  roleOptionsError,
  canFilterByRole,
  t,
}) {
  return (
    <div className="flex flex-col gap-3 border-b border-border bg-muted/25 p-4 xl:flex-row xl:items-end">
      <label className="grid min-w-0 flex-1 gap-1.5">
        <span className="text-xs font-medium">{t('adminAccounts.filters.searchLabel')}</span>
        <div className="relative">
          <MagnifyingGlassIcon
            className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
            size={16}
          />
          <Input
            className="pl-9"
            value={keyword}
            onChange={(event) => onKeywordChange(event.target.value)}
            placeholder={t('adminAccounts.filters.searchPlaceholder')}
            maxLength={100}
          />
        </div>
      </label>

      <label className="grid gap-1.5 xl:w-44">
        <span className="text-xs font-medium">{t('adminAccounts.filters.statusLabel')}</span>
        <select
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-[13px] shadow-xs outline-none focus:border-ring focus:ring-2 focus:ring-ring/20"
          value={status}
          onChange={(event) => onStatusChange(event.target.value)}
        >
          <option value="">{t('adminAccounts.filters.allStatuses')}</option>
          <option value="ENABLE">{t('adminAccounts.filters.enabled')}</option>
          <option value="DISABLE">{t('adminAccounts.filters.disabled')}</option>
        </select>
      </label>

      {canFilterByRole ? (
        <label className="grid gap-1.5 xl:w-56">
          <span className="text-xs font-medium">{t('adminAccounts.filters.roleLabel')}</span>
          <select
            className="h-9 w-full rounded-md border border-input bg-background px-3 text-[13px] shadow-xs outline-none focus:border-ring focus:ring-2 focus:ring-ring/20 disabled:cursor-not-allowed disabled:opacity-50"
            value={roleId}
            onChange={(event) => onRoleChange(event.target.value)}
            disabled={isLoadingRoles}
          >
            <option value="">
              {isLoadingRoles
                ? t('adminAccounts.filters.loadingRoles')
                : t('adminAccounts.filters.allRoles')}
            </option>
            {roleOptions.map((role) => (
              <option key={role.roleId} value={role.roleId}>{role.name}</option>
            ))}
          </select>
          {roleOptionsError ? (
            <span className="text-xs font-normal text-destructive">{roleOptionsError}</span>
          ) : null}
        </label>
      ) : null}

    </div>
  )
}

export { AdminAccountsFilters }
