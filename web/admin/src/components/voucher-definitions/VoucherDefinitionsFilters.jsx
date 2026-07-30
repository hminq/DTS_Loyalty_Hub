import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { FilterField } from '../data-list/FilterField'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'

function VoucherDefinitionsFilters({
  filters,
  onApply,
  onClear,
  options = {},
  isLoadingOptions = false,
  optionsError = '',
  onRetryOptions,
  presentation = 'popover',
}) {
  const { t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    setDraft(toDraft(filters))
  }, [filters.keyword, filters.rewardType, filters.validityType, filters.publishType])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()
    onApply({
      keyword: draft.keyword.trim(),
      rewardType: draft.rewardType,
      validityType: draft.validityType,
      publishType: draft.publishType,
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
      rewardType: filterKey === 'rewardType' ? '' : filters.rewardType,
      validityType: filterKey === 'validityType' ? '' : filters.validityType,
      publishType: filterKey === 'publishType' ? '' : filters.publishType,
    })
  }

  const activeFilters = getActiveFilterChips(filters, {
    options,
    t,
  })
  const hasActiveFilters = activeFilters.length > 0

  const form = (
    <form
      className={presentation === 'popover' ? 'grid gap-3' : 'mb-4 rounded-lg border border-border bg-muted/25 p-3'}
      onSubmit={submit}
    >
      <div className={presentation === 'popover' ? 'grid gap-3' : 'grid items-end gap-3 md:grid-cols-2 xl:grid-cols-4'}>
        <FilterField label={t('voucherDefinitions.filters.searchLabel')}>
          <Input
            value={draft.keyword}
            onChange={(event) => update('keyword', event.target.value)}
            placeholder={t('voucherDefinitions.filters.searchPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        <FilterField label={t('voucherDefinitions.filters.rewardTypeLabel')}>
          <Combobox
            value={draft.rewardType}
            onValueChange={(value) => update('rewardType', value)}
            options={options?.rewardTypes ?? []}
            placeholder={t('voucherDefinitions.filters.allRewardTypes')}
            emptyOptionLabel={t('voucherDefinitions.filters.allRewardTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('voucherDefinitions.filters.rewardTypeLabel')}
          />
        </FilterField>

        <FilterField label={t('voucherDefinitions.filters.validityTypeLabel')}>
          <Combobox
            value={draft.validityType}
            onValueChange={(value) => update('validityType', value)}
            options={options?.validityTypes ?? []}
            placeholder={t('voucherDefinitions.filters.allValidityTypes')}
            emptyOptionLabel={t('voucherDefinitions.filters.allValidityTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('voucherDefinitions.filters.validityTypeLabel')}
          />
        </FilterField>

        <FilterField label={t('voucherDefinitions.filters.publishTypeLabel')}>
          <Combobox
            value={draft.publishType}
            onValueChange={(value) => update('publishType', value)}
            options={options?.publishTypes ?? []}
            placeholder={t('voucherDefinitions.filters.allPublishTypes')}
            emptyOptionLabel={t('voucherDefinitions.filters.allPublishTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('voucherDefinitions.filters.publishTypeLabel')}
          />
        </FilterField>

        {optionsError ? (
          <div className="flex items-center gap-2">
            <span className="text-xs text-destructive">{optionsError}</span>
            <Button variant="outline" size="sm" type="button" onClick={onRetryOptions}>
              {t('common.retry')}
            </Button>
          </div>
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

  const triggerExtra = optionsError ? (
    <Button variant="outline" size="sm" type="button" onClick={onRetryOptions}>
      {t('common.retry')}
    </Button>
  ) : null

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
      triggerExtra={triggerExtra}
    >
      {form}
    </FilterPopoverControls>
  )
}

function toDraft(filters) {
  return {
    keyword: filters.keyword || '',
    rewardType: filters.rewardType || '',
    validityType: filters.validityType || '',
    publishType: filters.publishType || '',
  }
}

function getOptionLabel(optionsList, value) {
  const item = (optionsList || []).find((o) => o.value === value)
  return item ? item.label : value
}

function getActiveFilterChips(filters, { options, t }) {
  const chips = []

  if (filters.keyword) {
    chips.push({
      key: 'keyword',
      label: t('voucherDefinitions.filters.searchLabel'),
      value: filters.keyword,
      isText: true,
    })
  }

  if (filters.rewardType) {
    chips.push({
      key: 'rewardType',
      label: t('voucherDefinitions.filters.rewardTypeLabel'),
      value: getOptionLabel(options?.rewardTypes, filters.rewardType),
    })
  }

  if (filters.validityType) {
    chips.push({
      key: 'validityType',
      label: t('voucherDefinitions.filters.validityTypeLabel'),
      value: getOptionLabel(options?.validityTypes, filters.validityType),
    })
  }

  if (filters.publishType) {
    chips.push({
      key: 'publishType',
      label: t('voucherDefinitions.filters.publishTypeLabel'),
      value: getOptionLabel(options?.publishTypes, filters.publishType),
    })
  }

  return chips
}

function hasVoucherDefinitionFilters(filters) {
  return Boolean(
    filters.keyword
    || filters.rewardType
    || filters.validityType
    || filters.publishType,
  )
}

export { VoucherDefinitionsFilters, hasVoucherDefinitionFilters }
