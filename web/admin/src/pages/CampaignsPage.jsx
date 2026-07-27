import { MegaphoneIcon, PlusIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useSearchParams } from 'react-router-dom'

import { getCampaignOptions, getCampaigns } from '../api/campaignsApi'
import { CampaignsFilters, hasCampaignFilters } from '../components/campaigns/CampaignsFilters'
import { CampaignsTable } from '../components/campaigns/CampaignsTable'
import { mapCampaignOptions } from '../components/campaigns/campaignOptions'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { EmptyState } from '../components/data-list/EmptyState'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function CampaignsPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()
  const [searchParams, setSearchParams] = useSearchParams()

  const canCreate = hasPermission(PermissionCodes.Campaigns.Create)
  const [successMessage, setSuccessMessage] = useState(location.state?.successMessage ?? '')

  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const keyword = searchParams.get('keyword') || ''
  const status = searchParams.get('status') || ''
  const eventType = searchParams.get('eventType') || ''

  const filters = useMemo(() => ({
    keyword,
    status,
    eventType,
  }), [keyword, status, eventType])

  const [items, setItems] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [loadError, setLoadError] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const [rawOptions, setRawOptions] = useState({})
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')
  const [optionsRetryKey, setOptionsRetryKey] = useState(0)

  const options = useMemo(
    () => mapCampaignOptions(rawOptions, t),
    [rawOptions, t],
  )

  const hasActiveFilters = hasCampaignFilters(filters)

  const updateSearchParams = useCallback((updates, replace = false) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)

      Object.entries(updates).forEach(([key, value]) => {
        if (value === '' || value === null || value === undefined) next.delete(key)
        else next.set(key, String(value))
      })

      if (!next.has('page')) next.set('page', '1')
      if (!next.has('pageSize')) next.set('pageSize', String(pageSize))
      return next
    }, { replace })
  }, [pageSize, setSearchParams])

  useEffect(() => {
    if (location.state?.successMessage || location.state?.errorMessage) {
      if (location.state?.errorMessage) setLoadError(location.state.errorMessage)
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    if (searchParams.get('page') !== String(page) || searchParams.get('pageSize') !== String(pageSize)) {
      updateSearchParams({ page, pageSize }, true)
    }
  }, [page, pageSize, searchParams, updateSearchParams])

  useEffect(() => {
    const controller = new AbortController()

    async function loadOptions() {
      setIsLoadingOptions(true)
      setOptionsError('')
      try {
        const response = await getCampaignOptions(controller.signal)
        if (controller.signal.aborted) return
        setRawOptions(response ?? {})
      } catch (error) {
        if (!controller.signal.aborted) {
          setOptionsError(error.message || t('errors.loadCampaignOptions', { defaultValue: 'Failed to load campaign options.' }))
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoadingOptions(false)
        }
      }
    }

    loadOptions()
    return () => controller.abort()
  }, [optionsRetryKey, t])

  useEffect(() => {
    const controller = new AbortController()

    async function loadCampaignsList() {
      if (items.length === 0) {
        setIsLoading(true)
      } else {
        setIsRefreshing(true)
      }
      setLoadError('')

      try {
        const response = await getCampaigns({
          page,
          pageSize,
          keyword,
          status,
          eventType,
        }, controller.signal)

        if (controller.signal.aborted) return

        const nextMeta = response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 }
        if (nextMeta.totalPages > 0 && page > nextMeta.totalPages) {
          updateSearchParams({ page: nextMeta.totalPages }, true)
          return
        }

        setItems(response.data ?? [])
        setMeta(nextMeta)
      } catch (error) {
        if (!controller.signal.aborted) {
          setLoadError(error.message || t('errors.loadCampaigns', { defaultValue: 'Failed to load campaigns.' }))
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }

    loadCampaignsList()
    return () => controller.abort()
  }, [page, pageSize, keyword, status, eventType, refreshKey, t, updateSearchParams])

  function applyFilters(nextFilters) {
    updateSearchParams({ ...nextFilters, page: 1 })
  }

  function handleClearFilters() {
    updateSearchParams({
      keyword: '',
      status: '',
      eventType: '',
      page: 1,
    })
  }

  const showEmptyState = !isLoading && items.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('campaigns.eyebrow')}
        title={t('campaigns.title')}
        description={t('campaigns.description')}
        actions={canCreate ? (
          <Button size="sm" onClick={() => navigate('/campaigns/new')}>
            <PlusIcon size={15} weight="bold" />
            {t('campaigns.create')}
          </Button>
        ) : null}
      />

      {successMessage ? (
        <div className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </div>
      ) : null}

      {loadError ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{loadError}</p>
          <Button variant="outline" size="sm" onClick={() => setRefreshKey((k) => k + 1)}>
            {t('common.retry')}
          </Button>
        </div>
      ) : null}

      <div className="mt-5">
        <CampaignsFilters
          filters={filters}
          onApply={applyFilters}
          onClear={handleClearFilters}
          options={options}
          isLoadingOptions={isLoadingOptions}
          optionsError={optionsError}
          onRetryOptions={() => setOptionsRetryKey((k) => k + 1)}
          presentation="popover"
        />

        <DataTableCard>
          {!showEmptyState ? (
            <>
              <CampaignsTable
                items={items}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                language={i18n.resolvedLanguage}
                t={t}
              />
              <ListPagination
                meta={meta}
                onPageChange={(nextPage) => updateSearchParams({ page: nextPage })}
                onPageSizeChange={(nextPageSize) => updateSearchParams({ pageSize: nextPageSize, page: 1 })}
              />
            </>
          ) : (
            <EmptyState
              icon={MegaphoneIcon}
              title={t(hasActiveFilters ? 'campaigns.noResultsTitle' : 'campaigns.emptyTitle')}
              description={t(hasActiveFilters ? 'campaigns.noResultsDescription' : 'campaigns.emptyDescription')}
              filtered={hasActiveFilters}
              onClearSearch={handleClearFilters}
              t={t}
            />
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

export { CampaignsPage }
