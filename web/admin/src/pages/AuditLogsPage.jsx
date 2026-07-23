import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'

import { getAuditLogFilterOptions, getAuditLogs } from '../api/auditLogsApi'
import { AuditLogsFilters, hasFilters } from '../components/audit-logs/AuditLogsFilters'
import { AuditLogsTable } from '../components/audit-logs/AuditLogsTable'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { ListPagination } from '../components/data-list/ListPagination'
import { ClockIcon } from '@phosphor-icons/react'
import { EmptyState } from '../components/data-list/EmptyState'
import { PageHeader } from '../components/layout/PageHeader'

function AuditLogsPage() {
  const { i18n, t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const filters = useMemo(() => ({
    fromDate: searchParams.get('fromDate') || '',
    toDate: searchParams.get('toDate') || '',
    entityType: searchParams.get('entityType') || '',
    action: searchParams.get('action') || '',
  }), [searchParams])
  const [auditLogs, setAuditLogs] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [options, setOptions] = useState({ entityTypes: [], actions: [] })
  const [optionsError, setOptionsError] = useState('')

  const updateSearchParams = useCallback((updates, replace = false) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)
      Object.entries(updates).forEach(([key, value]) => {
        if (value === '' || value === null || value === undefined) next.delete(key)
        else next.set(key, String(value))
      })
      return next
    }, { replace })
  }, [setSearchParams])

  useEffect(() => {
    const controller = new AbortController()
    getAuditLogFilterOptions(controller.signal)
      .then((result) => setOptions(result ?? { entityTypes: [], actions: [] }))
      .catch((error) => {
        if (!controller.signal.aborted) setOptionsError(error.message || t('errors.loadAuditLogOptions'))
      })
    return () => controller.abort()
  }, [t])

  useEffect(() => {
    const controller = new AbortController()

    async function loadAuditLogs() {
      setIsLoading(true)
      setErrorMessage('')
      try {
        const response = await getAuditLogs({ page, pageSize, ...filters }, controller.signal)
        if (controller.signal.aborted) return
        const nextMeta = response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 }
        if (nextMeta.totalPages > 0 && page > nextMeta.totalPages) {
          updateSearchParams({ page: nextMeta.totalPages }, true)
          return
        }
        setAuditLogs(response.data ?? [])
        setMeta(nextMeta)
      } catch (error) {
        if (!controller.signal.aborted) setErrorMessage(error.message || t('errors.loadAuditLogs'))
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }
    }

    loadAuditLogs()
    return () => controller.abort()
  }, [filters, page, pageSize, t, updateSearchParams])

  function applyFilters(nextFilters) {
    updateSearchParams({ ...nextFilters, page: 1 })
  }

  function clearFilters() {
    updateSearchParams({ fromDate: '', toDate: '', entityType: '', action: '', page: 1 })
  }

  return (
    <>
      <PageHeader eyebrow={t('auditLogs.eyebrow')} title={t('auditLogs.title')} description={t('auditLogs.description')} />
      {errorMessage ? <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">{errorMessage}</p> : null}
      <div className="mt-5">
        <AuditLogsFilters filters={filters} options={options} optionsError={optionsError} onApply={applyFilters} onClear={clearFilters} />
        <DataTableCard>
          {!isLoading && auditLogs.length === 0 ? (
            <EmptyState
              icon={ClockIcon}
              title={t(hasFilters(filters) ? 'auditLogs.noResultsTitle' : 'auditLogs.emptyTitle')}
              description={t(hasFilters(filters) ? 'auditLogs.noResultsDescription' : 'auditLogs.emptyDescription')}
              filtered={hasFilters(filters)}
              onClearSearch={clearFilters}
              t={t}
            />
          ) : (
            <>
              {isLoading && auditLogs.length > 0 ? <p className="p-3 text-xs text-muted-foreground">{t('auditLogs.refreshing')}</p> : null}
              <AuditLogsTable auditLogs={auditLogs} isLoading={isLoading} language={i18n.resolvedLanguage} />
              <ListPagination
                meta={meta}
                onPageChange={(nextPage) => updateSearchParams({ page: nextPage })}
                onPageSizeChange={(nextPageSize) => updateSearchParams({ pageSize: nextPageSize, page: 1 })}
              />
            </>
          )}
        </DataTableCard>
      </div>
    </>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { AuditLogsPage }
