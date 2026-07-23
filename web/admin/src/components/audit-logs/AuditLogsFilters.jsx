import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'
import { DateTimePicker } from '../ui/date-time-picker'
import { Combobox } from '../ui/combobox'

function AuditLogsFilters({ filters, options, optionsError, onApply, onClear }) {
  const { t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [dateError, setDateError] = useState('')

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
    onApply({ fromDate, toDate, entityType: draft.entityType, action: draft.action })
  }

  return (
    <form className="mb-4 rounded-lg border border-border bg-muted/25 p-3" onSubmit={submit}>
      <div className="grid items-end gap-3 md:grid-cols-2 xl:grid-cols-[minmax(180px,1fr)_minmax(180px,1fr)_minmax(150px,0.8fr)_minmax(140px,0.7fr)_auto]">
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
          <Combobox value={draft.entityType} onValueChange={(value) => update('entityType', value)} options={(options.entityTypes ?? []).map((value) => ({ value, label: value }))} placeholder={t('auditLogs.filters.allEntityTypes')} emptyOptionLabel={t('auditLogs.filters.allEntityTypes')} searchPlaceholder={t('auditLogs.filters.searchEntityTypes')} emptyText={t('auditLogs.filters.noEntityTypes')} ariaLabel={t('auditLogs.filters.entityType')} />
        </FilterField>
        <FilterField label={t('auditLogs.filters.action')}>
          <Combobox value={draft.action} onValueChange={(value) => update('action', value)} options={(options.actions ?? []).map((value) => ({ value, label: value }))} placeholder={t('auditLogs.filters.allActions')} emptyOptionLabel={t('auditLogs.filters.allActions')} searchPlaceholder={t('auditLogs.filters.searchActions')} emptyText={t('auditLogs.filters.noActions')} ariaLabel={t('auditLogs.filters.action')} />
        </FilterField>
        <div className="flex h-9 items-center gap-2 md:col-span-2 xl:col-span-1">
          <Button type="submit" size="sm">{t('auditLogs.filters.apply')}</Button>
          {hasFilters(filters) ? <Button type="button" size="sm" variant="outline" onClick={onClear}>{t('auditLogs.filters.clear')}</Button> : null}
        </div>
      </div>
      {dateError ? <p className="mt-2 text-xs text-destructive">{dateError}</p> : null}
      {optionsError ? <p className="mt-2 text-xs text-destructive">{optionsError}</p> : null}
    </form>
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

function hasFilters(filters) {
  return Boolean(filters.fromDate || filters.toDate || filters.entityType || filters.action)
}

export { AuditLogsFilters, hasFilters }
