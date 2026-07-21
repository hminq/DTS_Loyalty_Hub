import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { searchRoleOptions } from '../../api/rolesApi'
import { Combobox } from '../ui/combobox'

const SEARCH_DEBOUNCE_MS = 300

function RoleSearchSelect({
  value,
  onChange,
  placeholder,
  emptyOptionLabel,
  disabled = false,
  invalid = false,
  ariaLabel,
}) {
  const { t } = useTranslation()
  const requestIdRef = useRef(0)
  const [isOpen, setIsOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [roles, setRoles] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [loadError, setLoadError] = useState('')

  useEffect(() => {
    if (!isOpen) return undefined

    const abortController = new AbortController()
    const requestId = requestIdRef.current + 1
    requestIdRef.current = requestId
    const timeoutId = window.setTimeout(async () => {
      setIsLoading(true)
      setLoadError('')
      try {
        const nextRoles = await searchRoleOptions(query, abortController.signal)
        if (requestId === requestIdRef.current) setRoles(nextRoles)
      } catch (error) {
        if (!abortController.signal.aborted && requestId === requestIdRef.current) {
          setRoles([])
          setLoadError(error.message || t('roleSelector.loadError'))
        }
      } finally {
        if (!abortController.signal.aborted && requestId === requestIdRef.current) setIsLoading(false)
      }
    }, query.trim() ? SEARCH_DEBOUNCE_MS : 0)

    return () => {
      window.clearTimeout(timeoutId)
      abortController.abort()
    }
  }, [isOpen, query, t])

  const options = roles.map((role) => ({ value: role.roleId, label: role.name, source: role }))

  return (
    <Combobox
      value={value}
      selectedLabel={value ? t('roleSelector.selectedFallback') : undefined}
      options={options}
      onValueChange={(nextValue, option) => onChange(nextValue, option?.source ?? null)}
      placeholder={placeholder}
      searchPlaceholder={t('roleSelector.searchPlaceholder')}
      emptyText={t('roleSelector.empty')}
      emptyOptionLabel={emptyOptionLabel}
      loadingText={t('roleSelector.loading')}
      error={loadError}
      isLoading={isLoading}
      disabled={disabled}
      invalid={invalid}
      ariaLabel={ariaLabel}
      searchValue={query}
      onSearchChange={setQuery}
      shouldFilter={false}
      onOpenChange={setIsOpen}
    />
  )
}

export { RoleSearchSelect }
