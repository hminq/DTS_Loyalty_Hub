import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { getVoucherDefinitionOptions } from '../../api/voucherDefinitionsApi'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { DateTimePicker } from '../ui/date-time-picker'
import { Input } from '../ui/input'
import { formatCustomerVoucherDateTime } from './customerVoucherFormatters'

function CustomerVoucherHistoryFilters({
  filters,
  includeCampaign = false,
  onApply,
  onClear,
  presentation = 'inline',
}) {
  const { i18n, t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [isOpen, setIsOpen] = useState(false)
  const [dateError, setDateError] = useState('')
  const [rewardTypeOptions, setRewardTypeOptions] = useState([])
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')
  const [optionsRetryKey, setOptionsRetryKey] = useState(0)

  useEffect(() => {
    setDraft(toDraft(filters))
    setDateError('')
  }, [
    filters.campaignName,
    filters.redeemAtFrom,
    filters.redeemAtTo,
    filters.rewardType,
    filters.userKeyword,
    filters.voucherKeyword,
  ])

  useEffect(() => {
    const controller = new AbortController()

    setIsLoadingOptions(true)
    setOptionsError('')

    getVoucherDefinitionOptions(controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return
        setRewardTypeOptions((result?.rewardTypes ?? []).map((value) => ({
          value,
          label: t(`customerVouchers.rewardTypes.${value}`, { defaultValue: value }),
        })))
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setOptionsError(error.message || t('customerVouchers.filters.loadRewardTypesError'))
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoadingOptions(false)
      })

    return () => controller.abort()
  }, [optionsRetryKey, t])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()

    if (
      draft.redeemAtFrom
      && draft.redeemAtTo
      && new Date(draft.redeemAtFrom) > new Date(draft.redeemAtTo)
    ) {
      setDateError(t('customerVouchers.filters.invalidRange'))
      return
    }

    setDateError('')
    onApply({
      voucherKeyword: draft.voucherKeyword.trim(),
      userKeyword: draft.userKeyword.trim(),
      rewardType: draft.rewardType,
      redeemAtFrom: normalizeUtcIso(draft.redeemAtFrom),
      redeemAtTo: normalizeUtcIso(draft.redeemAtTo),
      campaignName: includeCampaign ? draft.campaignName.trim() : '',
    })
    setIsOpen(false)
  }

  function clearAllFilters() {
    setDateError('')
    onClear()
    setIsOpen(false)
  }

  function removeFilter(filterKey) {
    setDateError('')
    onApply({
      voucherKeyword: filterKey === 'voucherKeyword' ? '' : filters.voucherKeyword,
      userKeyword: filterKey === 'userKeyword' ? '' : filters.userKeyword,
      rewardType: filterKey === 'rewardType' ? '' : filters.rewardType,
      redeemAtFrom: filterKey === 'redeemAtFrom' ? '' : filters.redeemAtFrom,
      redeemAtTo: filterKey === 'redeemAtTo' ? '' : filters.redeemAtTo,
      campaignName: includeCampaign && filterKey !== 'campaignName' ? filters.campaignName : '',
    })
  }

  const activeFilters = getActiveFilterChips(filters, {
    includeCampaign,
    language: i18n.resolvedLanguage,
    rewardTypeOptions,
    t,
  })
  const hasActiveFilters = activeFilters.length > 0

  const form = (
    <form
      className={presentation === 'popover' ? 'grid gap-3' : 'mb-4 rounded-lg border border-border bg-muted/25 p-3'}
      onSubmit={submit}
    >
      <div className={presentation === 'popover'
        ? 'grid gap-3'
        : 'grid items-end gap-3 md:grid-cols-2 xl:grid-cols-3'}
      >
        <FilterField label={t('customerVouchers.filters.voucherKeyword')}>
          <Input
            value={draft.voucherKeyword}
            onChange={(event) => update('voucherKeyword', event.target.value)}
            placeholder={t('customerVouchers.filters.voucherKeywordPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        <FilterField label={t('customerVouchers.filters.userKeyword')}>
          <Input
            value={draft.userKeyword}
            onChange={(event) => update('userKeyword', event.target.value)}
            placeholder={t('customerVouchers.filters.userKeywordPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        {includeCampaign ? (
          <FilterField label={t('voucherRedemptions.filters.campaignName')}>
            <Input
              value={draft.campaignName}
              onChange={(event) => update('campaignName', event.target.value)}
              placeholder={t('voucherRedemptions.filters.campaignNamePlaceholder')}
              maxLength={100}
            />
          </FilterField>
        ) : null}

        <FilterField label={t('customerVouchers.filters.rewardType')}>
          <Combobox
            value={draft.rewardType}
            onValueChange={(value) => update('rewardType', value)}
            options={rewardTypeOptions}
            placeholder={t('customerVouchers.filters.allRewardTypes')}
            emptyOptionLabel={t('customerVouchers.filters.allRewardTypes')}
            searchPlaceholder={t('customerVouchers.filters.searchRewardTypes')}
            emptyText={t('customerVouchers.filters.noRewardTypes')}
            loadingText={t('customerVouchers.filters.loadingRewardTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('customerVouchers.filters.rewardType')}
          />
        </FilterField>

        {presentation === 'popover' ? (
          <div className="grid gap-3 sm:grid-cols-2">
            <FilterField label={t('customerVouchers.filters.receivedFrom')}>
              <DateTimePicker
                value={draft.redeemAtFrom}
                onChange={(value) => update('redeemAtFrom', value)}
                placeholder={t('customerVouchers.filters.pickDateTime')}
                clearLabel={t('customerVouchers.filters.clearDate')}
              />
            </FilterField>

            <FilterField label={t('customerVouchers.filters.receivedTo')}>
              <DateTimePicker
                value={draft.redeemAtTo}
                onChange={(value) => update('redeemAtTo', value)}
                placeholder={t('customerVouchers.filters.pickDateTime')}
                clearLabel={t('customerVouchers.filters.clearDate')}
              />
            </FilterField>
          </div>
        ) : (
          <>
            <FilterField label={t('customerVouchers.filters.receivedFrom')}>
              <DateTimePicker
                value={draft.redeemAtFrom}
                onChange={(value) => update('redeemAtFrom', value)}
                placeholder={t('customerVouchers.filters.pickDateTime')}
                clearLabel={t('customerVouchers.filters.clearDate')}
              />
            </FilterField>

            <FilterField label={t('customerVouchers.filters.receivedTo')}>
              <DateTimePicker
                value={draft.redeemAtTo}
                onChange={(value) => update('redeemAtTo', value)}
                placeholder={t('customerVouchers.filters.pickDateTime')}
                clearLabel={t('customerVouchers.filters.clearDate')}
              />
            </FilterField>
          </>
        )}

        <div className="flex h-9 flex-wrap items-center gap-2">
          <Button type="submit" size="sm">
            {t('customerVouchers.filters.apply')}
          </Button>
          {hasActiveFilters ? (
            <Button type="button" variant="outline" size="sm" onClick={clearAllFilters}>
              {t('customerVouchers.filters.clearAll')}
            </Button>
          ) : null}
          {optionsError ? (
            <Button type="button" variant="outline" size="sm" onClick={() => setOptionsRetryKey((key) => key + 1)}>
              {t('common.retry')}
            </Button>
          ) : null}
        </div>
      </div>

      {dateError ? <p className="mt-2 text-xs font-medium text-destructive">{dateError}</p> : null}
    </form>
  )

  if (presentation !== 'popover') {
    return form
  }

  return (
    <FilterPopoverControls
      activeFilters={activeFilters}
      clearAllLabel={t('customerVouchers.filters.clearAll')}
      filterButtonLabel={t('customerVouchers.filters.filterButton')}
      isOpen={isOpen}
      onClearAll={clearAllFilters}
      onOpenChange={setIsOpen}
      onRemoveFilter={removeFilter}
      removeFilterLabel={(label) => t('customerVouchers.filters.removeFilter', { label })}
    >
      {form}
    </FilterPopoverControls>
  )
}

function FilterField({ label, children }) {
  return (
    <label className="grid min-w-0 gap-1.5">
      <span className="text-xs font-medium">{label}</span>
      {children}
    </label>
  )
}

function toDraft(filters) {
  return {
    voucherKeyword: filters.voucherKeyword || '',
    userKeyword: filters.userKeyword || '',
    rewardType: filters.rewardType || '',
    redeemAtFrom: normalizeUtcIso(filters.redeemAtFrom),
    redeemAtTo: normalizeUtcIso(filters.redeemAtTo),
    campaignName: filters.campaignName || '',
  }
}

function normalizeUtcIso(value) {
  if (!value) return ''
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '' : date.toISOString()
}

function hasCustomerVoucherFilters(filters, includeCampaign = false) {
  return Boolean(
    filters.voucherKeyword
    || filters.userKeyword
    || filters.rewardType
    || filters.redeemAtFrom
    || filters.redeemAtTo
    || (includeCampaign && filters.campaignName),
  )
}

function getActiveFilterChips(filters, { includeCampaign, language, rewardTypeOptions, t }) {
  const chips = []
  const rewardTypeOption = rewardTypeOptions.find((option) => option.value === filters.rewardType)

  if (filters.voucherKeyword) {
    chips.push({
      key: 'voucherKeyword',
      label: t('customerVouchers.filters.voucherKeyword'),
      value: filters.voucherKeyword,
      isText: true,
    })
  }

  if (filters.userKeyword) {
    chips.push({
      key: 'userKeyword',
      label: t('customerVouchers.filters.userKeyword'),
      value: filters.userKeyword,
      isText: true,
    })
  }

  if (filters.rewardType) {
    chips.push({
      key: 'rewardType',
      label: t('customerVouchers.filters.rewardType'),
      value: rewardTypeOption?.label || t(`customerVouchers.rewardTypes.${filters.rewardType}`, { defaultValue: filters.rewardType }),
    })
  }

  if (filters.redeemAtFrom) {
    chips.push({
      key: 'redeemAtFrom',
      label: t('customerVouchers.filters.receivedFrom'),
      value: formatCustomerVoucherDateTime(filters.redeemAtFrom, language),
    })
  }

  if (filters.redeemAtTo) {
    chips.push({
      key: 'redeemAtTo',
      label: t('customerVouchers.filters.receivedTo'),
      value: formatCustomerVoucherDateTime(filters.redeemAtTo, language),
    })
  }

  if (includeCampaign && filters.campaignName) {
    chips.push({
      key: 'campaignName',
      label: t('voucherRedemptions.filters.campaignName'),
      value: filters.campaignName,
      isText: true,
    })
  }

  return chips
}

export { CustomerVoucherHistoryFilters, hasCustomerVoucherFilters }
