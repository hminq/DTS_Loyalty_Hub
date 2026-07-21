import { MagnifyingGlassIcon } from '@phosphor-icons/react'

import { Input } from '../ui/input'
import { Combobox } from '../ui/combobox'
import { RoleSearchSelect } from '../roles/RoleSearchSelect'

function AdminAccountsFilters({
  keyword,
  onKeywordChange,
  status,
  onStatusChange,
  roleId,
  onRoleChange,
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
        <Combobox
          value={status}
          onValueChange={onStatusChange}
          options={[
            { value: 'ENABLE', label: t('adminAccounts.filters.enabled') },
            { value: 'DISABLE', label: t('adminAccounts.filters.disabled') },
          ]}
          placeholder={t('adminAccounts.filters.allStatuses')}
          emptyOptionLabel={t('adminAccounts.filters.allStatuses')}
          searchPlaceholder={t('adminAccounts.filters.searchStatus')}
          emptyText={t('adminAccounts.filters.noStatuses')}
          ariaLabel={t('adminAccounts.filters.statusLabel')}
        />
      </label>

      {canFilterByRole ? (
        <div className="grid gap-1.5 xl:w-56">
          <span className="text-xs font-medium">{t('adminAccounts.filters.roleLabel')}</span>
          <RoleSearchSelect
            value={roleId}
            onChange={onRoleChange}
            placeholder={t('adminAccounts.filters.allRoles')}
            emptyOptionLabel={t('adminAccounts.filters.allRoles')}
            ariaLabel={t('adminAccounts.filters.roleLabel')}
          />
        </div>
      ) : null}

    </div>
  )
}

export { AdminAccountsFilters }
