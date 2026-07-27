import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { FilterField } from '../data-list/FilterField'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'

function CampaignsFilters({
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
  }, [filters.keyword, filters.status, filters.eventType])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()
    onApply({
      keyword: draft.keyword.trim(),
      status: draft.status,
      eventType: draft.eventType,
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
      eventType: filterKey === 'eventType' ? '' : filters.eventType,
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
      <div className={presentation === 'popover' ? 'grid gap-3' : 'grid items-end gap-3 md:grid-cols-2 xl:grid-cols-3'}>
        <FilterField label={t('campaigns.filters.searchLabel')}>
          <Input
            value={draft.keyword}
            onChange={(event) => update('keyword', event.target.value)}
            placeholder={t('campaigns.filters.searchPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        <FilterField label={t('campaigns.filters.statusLabel')}>
          <Combobox
            value={draft.status}
            onValueChange={(value) => update('status', value)}
            options={options?.campaignStatuses ?? []}
            placeholder={t('campaigns.filters.allStatuses')}
            emptyOptionLabel={t('campaigns.filters.allStatuses')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('campaigns.filters.statusLabel')}
          />
        </FilterField>

        <FilterField label={t('campaigns.filters.eventTypeLabel')}>
          <Combobox
            value={draft.eventType}
            onValueChange={(value) => update('eventType', value)}
            options={options?.eventTypes ?? []}
            placeholder={t('campaigns.filters.allEventTypes')}
            emptyOptionLabel={t('campaigns.filters.allEventTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('campaigns.filters.eventTypeLabel')}
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
    status: filters.status || '',
    eventType: filters.eventType || '',
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
      label: t('campaigns.filters.searchLabel'),
      value: filters.keyword,
      isText: true,
    })
  }

  if (filters.status) {
    chips.push({
      key: 'status',
      label: t('campaigns.filters.statusLabel'),
      value: getOptionLabel(options?.campaignStatuses, filters.status),
    })
  }

  if (filters.eventType) {
    chips.push({
      key: 'eventType',
      label: t('campaigns.filters.eventTypeLabel'),
      value: getOptionLabel(options?.eventTypes, filters.eventType),
    })
  }

  return chips
}

function hasCampaignFilters(filters) {
  return Boolean(
    filters.keyword
    || filters.status
    || filters.eventType,
  )
}

export { CampaignsFilters, hasCampaignFilters }
