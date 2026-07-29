import { PlusIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useOutletContext, useSearchParams } from 'react-router-dom'

import { getEventDefinitionOptions, getEventDefinitions } from '../api/eventDefinitionsApi'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { ListPagination } from '../components/data-list/ListPagination'
import { EventDefinitionsFilters } from '../components/event-definitions/EventDefinitionsFilters'
import { EventDefinitionsTable } from '../components/event-definitions/EventDefinitionsTable'
import { mapEventDefinitionOptions } from '../components/event-definitions/eventDefinitionOptions'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

export function EventDefinitionsPage() {
  const { t } = useTranslation()
  const { hasPermission } = useOutletContext()
  const [searchParams, setSearchParams] = useSearchParams()

  const page = parseInt(searchParams.get('page') || '1', 10)
  const pageSize = parseInt(searchParams.get('pageSize') || '20', 10)
  const keyword = searchParams.get('keyword') || ''
  const status = searchParams.get('status') || ''

  const [items, setItems] = useState([])
  const [meta, setMeta] = useState({ page: 1, pageSize: 20, totalItems: 0, totalPages: 1 })
  const [optionsData, setOptionsData] = useState({})
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')
  const [optionsRetryKey, setOptionsRetryKey] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [error, setError] = useState(null)

  const canCreate = hasPermission(PermissionCodes.EventDefinitions.Create)

  const options = useMemo(() => mapEventDefinitionOptions(optionsData, t), [optionsData, t])

  const fetchDefinitions = useCallback(
    async (isSilent = false) => {
      if (isSilent) {
        setIsRefreshing(true)
      } else {
        setIsLoading(true)
      }
      setError(null)

      try {
        const res = await getEventDefinitions({ page, pageSize, keyword, status })
        setItems(res.data || [])
        if (res.meta) {
          setMeta(res.meta)
        }
      } catch (err) {
        setError(err?.message || t('errors.unexpected'))
      } finally {
        setIsLoading(false)
        setIsRefreshing(false)
      }
    },
    [page, pageSize, keyword, status, t],
  )

  useEffect(() => {
    let isMounted = true
    setIsLoadingOptions(true)
    setOptionsError('')

    getEventDefinitionOptions()
      .then((opts) => {
        if (isMounted) setOptionsData(opts || {})
      })
      .catch((err) => {
        if (isMounted) setOptionsError(err?.message || t('errors.unexpected'))
      })
      .finally(() => {
        if (isMounted) setIsLoadingOptions(false)
      })

    return () => {
      isMounted = false
    }
  }, [optionsRetryKey, t])

  useEffect(() => {
    fetchDefinitions()
  }, [fetchDefinitions])

  const updateFilters = (newParams) => {
    setSearchParams((prev) => {
      const updated = new URLSearchParams(prev)
      Object.entries(newParams).forEach(([key, val]) => {
        if (val) {
          updated.set(key, val)
        } else {
          updated.delete(key)
        }
      })
      updated.set('page', '1')
      return updated
    })
  }

  const handleClearFilters = () => setSearchParams({ page: '1', pageSize: String(pageSize) })

  const handlePageChange = (newPage) => {
    setSearchParams((prev) => {
      const updated = new URLSearchParams(prev)
      updated.set('page', newPage.toString())
      return updated
    })
  }

  const handlePageSizeChange = (newPageSize) => {
    setSearchParams((prev) => {
      const updated = new URLSearchParams(prev)
      updated.set('pageSize', newPageSize.toString())
      updated.set('page', '1')
      return updated
    })
  }

  const filters = useMemo(() => ({ keyword, status }), [keyword, status])

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('eventDefinitions.title')}
        description={t('eventDefinitions.description')}
        actions={
          canCreate ? (
            <Link to="/event-definitions/new">
              <Button>
                <PlusIcon data-icon="inline-start" />
                {t('eventDefinitions.create')}
              </Button>
            </Link>
          ) : null
        }
      />

      {error && (
        <div className="rounded-md bg-destructive/10 p-4 text-sm font-medium text-destructive">
          {error}
        </div>
      )}

      <EventDefinitionsFilters
        filters={filters}
        onApply={updateFilters}
        onClear={handleClearFilters}
        options={options}
        isLoadingOptions={isLoadingOptions}
        optionsError={optionsError}
        onRetryOptions={() => setOptionsRetryKey((key) => key + 1)}
      />

      <DataTableCard>
        <EventDefinitionsTable
          items={items}
          isLoading={isLoading}
          isRefreshing={isRefreshing}
        />
        <ListPagination
          meta={meta}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      </DataTableCard>
    </div>
  )
}
