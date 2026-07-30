import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { FilterField } from '../data-list/FilterField'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Input } from '../ui/input'

function NotificationTemplatesFilters({
  filters = {},
  onApply,
  onClear,
  presentation = 'popover',
}) {
  const { t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    setDraft(toDraft(filters))
  }, [filters.keyword, filters.notificationCode])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()
    onApply?.({
      keyword: draft.keyword.trim(),
      notificationCode: draft.notificationCode.trim(),
    })
    setIsOpen(false)
  }

  function clearAllFilters() {
    onClear?.()
    setIsOpen(false)
  }

  function removeFilter(filterKey) {
    onApply?.({
      keyword: filterKey === 'keyword' ? '' : filters.keyword,
      notificationCode: filterKey === 'notificationCode' ? '' : filters.notificationCode,
    })
  }

  const activeFilters = getActiveFilterChips(filters, { t })
  const hasActiveFilters = activeFilters.length > 0

  const form = (
    <form
      className={presentation === 'popover' ? 'grid gap-3' : 'mb-4 rounded-lg border border-border bg-muted/25 p-3'}
      onSubmit={submit}
    >
      <div className={presentation === 'popover' ? 'grid gap-3' : 'grid items-end gap-3 md:grid-cols-2'}>
        <FilterField label={t('notifications.filters.searchLabel')}>
          <Input
            value={draft.keyword}
            onChange={(event) => update('keyword', event.target.value)}
            placeholder={t('notifications.filters.searchPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        <FilterField label={t('notifications.filters.notificationCodeLabel')}>
          <Input
            value={draft.notificationCode}
            onChange={(event) => update('notificationCode', event.target.value)}
            placeholder={t('notifications.filters.notificationCodePlaceholder')}
            maxLength={100}
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

function toDraft(filters = {}) {
  return {
    keyword: filters.keyword || '',
    notificationCode: filters.notificationCode || '',
  }
}

function getActiveFilterChips(filters = {}, { t }) {
  const chips = []

  if (filters.keyword) {
    chips.push({
      key: 'keyword',
      label: t('notifications.filters.searchLabel'),
      value: filters.keyword,
      isText: true,
    })
  }

  if (filters.notificationCode) {
    chips.push({
      key: 'notificationCode',
      label: t('notifications.filters.notificationCodeLabel'),
      value: filters.notificationCode,
      isText: true,
    })
  }

  return chips
}

export { NotificationTemplatesFilters }
