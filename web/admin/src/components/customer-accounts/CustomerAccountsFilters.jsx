import { MagnifyingGlassIcon } from '@phosphor-icons/react'

import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'

function CustomerAccountsFilters({
  keyword,
  onKeywordChange,
  status,
  onStatusChange,
  tierId,
  onTierChange,
  canFilterByTier,
  tierOptions,
  isTierLoading,
  tierError,
  t,
}) {
  return (
    <div className="mb-4 flex flex-col gap-3 rounded-lg border border-border bg-muted/25 p-3 xl:flex-row xl:items-end">
      <label className="grid min-w-0 flex-1 gap-1.5">
        <span className="text-xs font-medium">{t('customerAccounts.filters.searchLabel')}</span>
        <div className="relative">
          <MagnifyingGlassIcon
            className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
            size={16}
            aria-hidden="true"
          />
          <Input
            className="pl-9"
            value={keyword}
            onChange={(event) => onKeywordChange(event.target.value)}
            placeholder={t('customerAccounts.filters.searchPlaceholder')}
            maxLength={100}
          />
        </div>
      </label>

      <label className="grid gap-1.5 xl:w-44">
        <span className="text-xs font-medium">{t('customerAccounts.filters.statusLabel')}</span>
        <Combobox
          value={status}
          onValueChange={onStatusChange}
          options={[
            { value: 'ENABLE', label: t('customerAccounts.status.enabled') },
            { value: 'DISABLE', label: t('customerAccounts.status.disabled') },
          ]}
          placeholder={t('customerAccounts.filters.allStatuses')}
          emptyOptionLabel={t('customerAccounts.filters.allStatuses')}
          searchPlaceholder={t('customerAccounts.filters.searchStatus')}
          emptyText={t('customerAccounts.filters.noStatuses')}
          ariaLabel={t('customerAccounts.filters.statusLabel')}
        />
      </label>

      {canFilterByTier ? (
        <label className="grid gap-1.5 xl:w-56">
          <span className="text-xs font-medium">{t('customerAccounts.filters.tierLabel')}</span>
          <Combobox
            value={tierId}
            onValueChange={onTierChange}
            options={tierOptions}
            placeholder={t('customerAccounts.filters.allTiers')}
            emptyOptionLabel={t('customerAccounts.filters.allTiers')}
            searchPlaceholder={t('customerAccounts.filters.searchTier')}
            emptyText={t('customerAccounts.filters.noTiers')}
            loadingText={t('customerAccounts.filters.loadingTiers')}
            error={tierError}
            isLoading={isTierLoading}
            ariaLabel={t('customerAccounts.filters.tierLabel')}
          />
        </label>
      ) : null}
    </div>
  )
}

export { CustomerAccountsFilters }
