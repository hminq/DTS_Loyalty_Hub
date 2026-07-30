import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { FilterField } from '../data-list/FilterField'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'

function CustomerAccountsFilters({
  filters,
  onApply,
  onClear,
  canFilterByTier = false,
  tierOptions = [],
  isTierLoading = false,
  tierError = '',
  presentation = 'popover',
}) {
  const { t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    setDraft(toDraft(filters))
  }, [filters.keyword, filters.status, filters.tierId])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()
    onApply({
      keyword: draft.keyword.trim(),
      status: draft.status,
      tierId: canFilterByTier ? draft.tierId : '',
    })
    setIsOpen(false)
  }

  function clearAllFilters() {
    onClear()
    setIsOpen(false)
  }

  function removeFilter(filterKey) {
    onApply({
      keyword: filterKey === 'keyword' ? '' : filters.keyword,
      status: filterKey === 'status' ? '' : filters.status,
      tierId: canFilterByTier && filterKey !== 'tierId' ? filters.tierId : '',
    })
  }

  const activeFilters = getActiveFilterChips(filters, {
    canFilterByTier,
    tierOptions,
    t,
  })
  const hasActiveFilters = activeFilters.length > 0

  const form = (
    <form
      className={presentation === 'popover' ? 'grid gap-3' : 'mb-4 rounded-lg border border-border bg-muted/25 p-3'}
      onSubmit={submit}
    >
      <div className={presentation === 'popover' ? 'grid gap-3' : 'grid items-end gap-3 md:grid-cols-2 xl:grid-cols-3'}>
        <FilterField label={t('customerAccounts.filters.searchLabel')}>
          <Input
            value={draft.keyword}
            onChange={(event) => update('keyword', event.target.value)}
            placeholder={t('customerAccounts.filters.searchPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        <FilterField label={t('customerAccounts.filters.statusLabel')}>
          <Combobox
            value={draft.status}
            onValueChange={(value) => update('status', value)}
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
        </FilterField>

        {canFilterByTier ? (
          <FilterField label={t('customerAccounts.filters.tierLabel')}>
            <Combobox
              value={draft.tierId}
              onValueChange={(value) => update('tierId', value)}
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
          </FilterField>
        ) : null}

        <div className="flex h-9 flex-wrap items-center gap-2">
          <Button type="submit" size="sm">
            {t('common.filters.apply')}
          </Button>
          {hasActiveFilters ? (
            <Button type="button" variant="outline" size="sm" onClick={clearAllFilters}>
              {t('common.filters.clearAll')}
            </Button>
          ) : null}
        </div>
      </div>
    </form>
  )

  if (presentation !== 'popover') {
    return form
  }

  return (
    <FilterPopoverControls
      activeFilters={activeFilters}
      clearAllLabel={t('common.filters.clearAll')}
      filterButtonLabel={t('common.filters.filter')}
      isOpen={isOpen}
      onClearAll={clearAllFilters}
      onOpenChange={setIsOpen}
      onRemoveFilter={removeFilter}
      removeFilterLabel={(label) => t('common.filters.removeFilter', { label })}
    >
      {form}
    </FilterPopoverControls>
  )
}

function toDraft(filters) {
  return {
    keyword: filters.keyword || '',
    status: filters.status || '',
    tierId: filters.tierId || '',
  }
}

function getActiveFilterChips(filters, { canFilterByTier, tierOptions, t }) {
  const chips = []

  if (filters.keyword) {
    chips.push({
      key: 'keyword',
      label: t('customerAccounts.filters.searchLabel'),
      value: filters.keyword,
      isText: true,
    })
  }

  if (filters.status) {
    const statusLabel =
      filters.status === 'ENABLE'
        ? t('customerAccounts.status.enabled')
        : filters.status === 'DISABLE'
          ? t('customerAccounts.status.disabled')
          : filters.status

    chips.push({
      key: 'status',
      label: t('customerAccounts.filters.statusLabel'),
      value: statusLabel,
    })
  }

  if (canFilterByTier && filters.tierId) {
    const tierOption = tierOptions.find((o) => o.value === filters.tierId)
    chips.push({
      key: 'tierId',
      label: t('customerAccounts.filters.tierLabel'),
      value: tierOption?.label || filters.tierId,
    })
  }

  return chips
}

function hasCustomerAccountFilters(filters, canFilterByTier = true) {
  return Boolean(
    filters.keyword
    || filters.status
    || (canFilterByTier && filters.tierId),
  )
}

export { CustomerAccountsFilters, hasCustomerAccountFilters }
