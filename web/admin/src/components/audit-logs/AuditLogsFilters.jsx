import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { FilterField } from '../data-list/FilterField'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { DateTimePicker } from '../ui/date-time-picker'

function AuditLogsFilters({
  filters,
  options = {},
  optionsError = '',
  onApply,
  onClear,
  presentation = 'popover',
}) {
  const { i18n, t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [dateError, setDateError] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    setDraft(toDraft(filters))
    setDateError('')
  }, [filters.action, filters.entityType, filters.fromDate, filters.toDate])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()
    const fromDate = normalizeUtcIso(draft.fromDate)
    const toDate = normalizeUtcIso(draft.toDate)

    if (fromDate && toDate && new Date(fromDate) > new Date(toDate)) {
      setDateError(t('auditLogs.filters.invalidRange'))
      return
    }

    setDateError('')
    onApply({
      fromDate,
      toDate,
      entityType: draft.entityType,
      action: draft.action,
    })
    setIsOpen(false)
  }

  function clearAllFilters() {
    onClear()
    setIsOpen(false)
  }

  function removeFilter(filterKey) {
    onApply({
      fromDate: filterKey === 'fromDate' ? '' : filters.fromDate,
      toDate: filterKey === 'toDate' ? '' : filters.toDate,
      entityType: filterKey === 'entityType' ? '' : filters.entityType,
      action: filterKey === 'action' ? '' : filters.action,
    })
  }

  const activeFilters = getActiveFilterChips(filters, {
    language: i18n.resolvedLanguage,
    t,
  })
  const hasActiveFilters = activeFilters.length > 0

  const form = (
    <form
      className={presentation === 'popover' ? 'grid gap-3' : 'mb-4 rounded-lg border border-border bg-muted/25 p-3'}
      onSubmit={submit}
    >
      <div className={presentation === 'popover' ? 'grid gap-3' : 'grid items-end gap-3 md:grid-cols-2 xl:grid-cols-[minmax(180px,1fr)_minmax(180px,1fr)_minmax(150px,0.8fr)_minmax(140px,0.7fr)_auto]'}>
        <FilterField label={t('auditLogs.filters.from')}>
          <DateTimePicker
            value={draft.fromDate}
            onChange={(value) => update('fromDate', value)}
            placeholder={t('auditLogs.filters.pickDateTime')}
            clearLabel={t('auditLogs.filters.clearDate')}
          />
        </FilterField>

        <FilterField label={t('auditLogs.filters.to')}>
          <DateTimePicker
            value={draft.toDate}
            onChange={(value) => update('toDate', value)}
            placeholder={t('auditLogs.filters.pickDateTime')}
            clearLabel={t('auditLogs.filters.clearDate')}
          />
        </FilterField>

        <FilterField label={t('auditLogs.filters.entityType')}>
          <Combobox
            value={draft.entityType}
            onValueChange={(value) => update('entityType', value)}
            options={(options.entityTypes ?? []).map((value) => ({ value, label: value }))}
            placeholder={t('auditLogs.filters.allEntityTypes')}
            emptyOptionLabel={t('auditLogs.filters.allEntityTypes')}
            searchPlaceholder={t('auditLogs.filters.searchEntityTypes')}
            emptyText={t('auditLogs.filters.noEntityTypes')}
            ariaLabel={t('auditLogs.filters.entityType')}
          />
        </FilterField>

        <FilterField label={t('auditLogs.filters.action')}>
          <Combobox
            value={draft.action}
            onValueChange={(value) => update('action', value)}
            options={(options.actions ?? []).map((value) => ({ value, label: value }))}
            placeholder={t('auditLogs.filters.allActions')}
            emptyOptionLabel={t('auditLogs.filters.allActions')}
            searchPlaceholder={t('auditLogs.filters.searchActions')}
            emptyText={t('auditLogs.filters.noActions')}
            ariaLabel={t('auditLogs.filters.action')}
          />
        </FilterField>

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

      {dateError ? <p className="mt-2 text-xs text-destructive">{dateError}</p> : null}
      {optionsError ? <p className="mt-2 text-xs text-destructive">{optionsError}</p> : null}
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
    fromDate: normalizeUtcIso(filters.fromDate),
    toDate: normalizeUtcIso(filters.toDate),
    entityType: filters.entityType || '',
    action: filters.action || '',
  }
}

function normalizeUtcIso(value) {
  if (!value) return ''
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '' : date.toISOString()
}

function formatChipDateTime(value, language) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  try {
    return new Intl.DateTimeFormat(language || 'en', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(date)
  } catch {
    return value
  }
}

function getActiveFilterChips(filters, { language, t }) {
  const chips = []

  if (filters.fromDate) {
    chips.push({
      key: 'fromDate',
      label: t('auditLogs.filters.from'),
      value: formatChipDateTime(filters.fromDate, language),
    })
  }

  if (filters.toDate) {
    chips.push({
      key: 'toDate',
      label: t('auditLogs.filters.to'),
      value: formatChipDateTime(filters.toDate, language),
    })
  }

  if (filters.entityType) {
    chips.push({
      key: 'entityType',
      label: t('auditLogs.filters.entityType'),
      value: filters.entityType,
    })
  }

  if (filters.action) {
    chips.push({
      key: 'action',
      label: t('auditLogs.filters.action'),
      value: filters.action,
    })
  }

  return chips
}

function hasFilters(filters) {
  return Boolean(filters.fromDate || filters.toDate || filters.entityType || filters.action)
}

export { AuditLogsFilters, hasFilters }
