import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { getRole } from '../../api/rolesApi'
import { FilterField } from '../data-list/FilterField'
import { FilterPopoverControls } from '../data-list/FilterPopoverControls'
import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'
import { RoleSearchSelect } from '../roles/RoleSearchSelect'

function AdminAccountsFilters({
  filters,
  onApply,
  onClear,
  canFilterByRole = false,
  presentation = 'popover',
}) {
  const { t } = useTranslation()
  const [draft, setDraft] = useState(() => toDraft(filters))
  const [isOpen, setIsOpen] = useState(false)
  const [roleNames, setRoleNames] = useState({})

  useEffect(() => {
    setDraft(toDraft(filters))
  }, [filters.keyword, filters.status, filters.roleId])

  useEffect(() => {
    if (!canFilterByRole || !filters.roleId || roleNames[filters.roleId]) {
      return undefined
    }

    let isAborted = false
    getRole(filters.roleId)
      .then((role) => {
        if (!isAborted && role?.name) {
          setRoleNames((current) => ({ ...current, [filters.roleId]: role.name }))
        }
      })
      .catch(() => {})

    return () => {
      isAborted = true
    }
  }, [canFilterByRole, filters.roleId, roleNames])

  function update(name, value) {
    setDraft((current) => ({ ...current, [name]: value }))
  }

  function handleRoleChange(value, roleObject) {
    update('roleId', value)
    if (value && roleObject?.name) {
      setRoleNames((current) => ({ ...current, [value]: roleObject.name }))
    }
  }

  function submit(event) {
    event.preventDefault()
    onApply({
      keyword: draft.keyword.trim(),
      status: draft.status,
      roleId: canFilterByRole ? draft.roleId : '',
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
      roleId: canFilterByRole && filterKey !== 'roleId' ? filters.roleId : '',
    })
  }

  const activeFilters = getActiveFilterChips(filters, {
    canFilterByRole,
    roleNames,
    t,
  })
  const hasActiveFilters = activeFilters.length > 0

  const form = (
    <form
      className={presentation === 'popover' ? 'grid gap-3' : 'mb-4 rounded-lg border border-border bg-muted/25 p-3'}
      onSubmit={submit}
    >
      <div className={presentation === 'popover' ? 'grid gap-3' : 'grid items-end gap-3 md:grid-cols-2 xl:grid-cols-3'}>
        <FilterField label={t('adminAccounts.filters.searchLabel')}>
          <Input
            value={draft.keyword}
            onChange={(event) => update('keyword', event.target.value)}
            placeholder={t('adminAccounts.filters.searchPlaceholder')}
            maxLength={100}
          />
        </FilterField>

        <FilterField label={t('adminAccounts.filters.statusLabel')}>
          <Combobox
            value={draft.status}
            onValueChange={(value) => update('status', value)}
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
        </FilterField>

        {canFilterByRole ? (
          <FilterField label={t('adminAccounts.filters.roleLabel')}>
            <RoleSearchSelect
              value={draft.roleId}
              selectedLabel={draft.roleId ? (roleNames[draft.roleId] || t('roleSelector.selectedFallback')) : undefined}
              onChange={handleRoleChange}
              placeholder={t('adminAccounts.filters.allRoles')}
              emptyOptionLabel={t('adminAccounts.filters.allRoles')}
              ariaLabel={t('adminAccounts.filters.roleLabel')}
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
    roleId: filters.roleId || '',
  }
}

function getActiveFilterChips(filters, { canFilterByRole, roleNames, t }) {
  const chips = []

  if (filters.keyword) {
    chips.push({
      key: 'keyword',
      label: t('adminAccounts.filters.searchLabel'),
      value: filters.keyword,
      isText: true,
    })
  }

  if (filters.status) {
    const statusLabel =
      filters.status === 'ENABLE'
        ? t('adminAccounts.filters.enabled')
        : filters.status === 'DISABLE'
          ? t('adminAccounts.filters.disabled')
          : filters.status

    chips.push({
      key: 'status',
      label: t('adminAccounts.filters.statusLabel'),
      value: statusLabel,
    })
  }

  if (canFilterByRole && filters.roleId) {
    chips.push({
      key: 'roleId',
      label: t('adminAccounts.filters.roleLabel'),
      value: roleNames[filters.roleId] || t('roleSelector.selectedFallback'),
    })
  }

  return chips
}

function hasAdminAccountFilters(filters, canFilterByRole = true) {
  return Boolean(
    filters.keyword
    || filters.status
    || (canFilterByRole && filters.roleId),
  )
}

export { AdminAccountsFilters, hasAdminAccountFilters }
