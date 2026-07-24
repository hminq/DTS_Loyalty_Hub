import { PlusIcon, TicketIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import * as React from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useSearchParams } from 'react-router-dom'

import { getVoucherDefinitionOptions, getVoucherDefinitions } from '../api/voucherDefinitionsApi'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { VoucherDefinitionsFilters } from '../components/voucher-definitions/VoucherDefinitionsFilters'
import { VoucherDefinitionsTable } from '../components/voucher-definitions/VoucherDefinitionsTable'
import { mapVoucherDefinitionOptions } from '../components/voucher-definitions/voucherDefinitionOptions'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function VoucherDefinitionsPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()
  const [searchParams, setSearchParams] = useSearchParams()

  const page = readPositiveInteger(searchParams.get('page'), 1)
  const requestedPageSize = readPositiveInteger(searchParams.get('pageSize'), 20)
  const pageSize = Math.min(requestedPageSize, 100)
  const keyword = searchParams.get('keyword') || ''
  const rewardType = searchParams.get('rewardType') || ''
  const validityType = searchParams.get('validityType') || ''
  const publishType = searchParams.get('publishType') || ''

  const [keywordInput, setKeywordInput] = useState(keyword)
  const [items, setItems] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [loadError, setLoadError] = useState('')
  const [noticeMessage, setNoticeMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)
  
  const [rawOptions, setRawOptions] = useState({})
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')
  const [optionsRetryKey, setOptionsRetryKey] = useState(0)

  const options = React.useMemo(
    () => mapVoucherDefinitionOptions(rawOptions, t),
    [rawOptions, t]
  )

  const canCreate = hasPermission(PermissionCodes.VoucherDefinitions.Create)

  const hasActiveFilters = Boolean(keyword || rewardType || validityType || publishType)

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
    setKeywordInput(keyword)
  }, [keyword])

  useEffect(() => {
    if (location.state?.errorMessage) {
      setLoadError(location.state.errorMessage)
      window.history.replaceState({}, document.title)
    }
    if (location.state?.noticeMessage) {
      setNoticeMessage(location.state.noticeMessage)
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    if (searchParams.get('page') !== String(page) || searchParams.get('pageSize') !== String(pageSize)) {
      updateSearchParams({ page, pageSize }, true)
    }
  }, [page, pageSize, searchParams, updateSearchParams])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      const normalizedKeyword = keywordInput.trim()
      if (normalizedKeyword !== keyword) {
        updateSearchParams({ keyword: normalizedKeyword, page: 1 })
      }
    }, 300)

    return () => window.clearTimeout(timeoutId)
  }, [keyword, keywordInput, updateSearchParams])

  useEffect(() => {
    const controller = new AbortController()

    async function loadOptions() {
      setIsLoadingOptions(true)
      setOptionsError('')

      try {
        const data = await getVoucherDefinitionOptions(controller.signal)
        if (controller.signal.aborted) return
        setRawOptions(data)
      } catch (error) {
        if (controller.signal.aborted) return
        setOptionsError(error.message || t('voucherDefinitions.errors.loadOptions'))
      } finally {
        if (!controller.signal.aborted) setIsLoadingOptions(false)
      }
    }

    loadOptions()

    return () => controller.abort()
  }, [optionsRetryKey, t])

  useEffect(() => {
    const controller = new AbortController()

    async function loadVoucherDefinitions() {
      if (items.length === 0) setIsLoading(true)
      else setIsRefreshing(true)
      setLoadError('')

      try {
        const response = await getVoucherDefinitions(
          { page, pageSize, keyword, rewardType, validityType, publishType },
          controller.signal,
        )
        if (controller.signal.aborted) return

        const nextMeta = response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 }
        setItems(response.data ?? [])
        setMeta(nextMeta)

        if (nextMeta.totalPages > 0 && page > nextMeta.totalPages) {
          updateSearchParams({ page: nextMeta.totalPages }, true)
        }
      } catch (error) {
        if (controller.signal.aborted) return
        setLoadError(error.message || t('voucherDefinitions.errors.loadList'))
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }

    loadVoucherDefinitions()

    return () => controller.abort()
  }, [page, pageSize, keyword, rewardType, validityType, publishType, refreshKey, t, updateSearchParams])

  const handleClearFilters = useCallback(() => {
    setKeywordInput('')
    updateSearchParams({ keyword: '', rewardType: '', validityType: '', publishType: '', page: 1 })
  }, [updateSearchParams])

  const handleViewDetail = useCallback((id) => {
    navigate(`/voucher-definitions/${id}`, {
      state: { returnSearch: searchParams.toString() },
    })
  }, [navigate, searchParams])

  const showEmptyState = !isLoading && items.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('voucherDefinitions.eyebrow')}
        title={t('voucherDefinitions.title')}
        description={t('voucherDefinitions.description')}
        actions={canCreate ? (
          <Button size="sm" onClick={() => navigate('/voucher-definitions/new')}>
            <PlusIcon size={15} weight="bold" />
            {t('voucherDefinitions.create')}
          </Button>
        ) : null}
      />

      {noticeMessage ? (
        <p className="mt-5 rounded-lg border border-info/20 bg-info/10 px-4 py-3 text-[13px] font-medium text-foreground">
          {noticeMessage}
        </p>
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
        <VoucherDefinitionsFilters
          keyword={keywordInput}
          rewardType={rewardType}
          validityType={validityType}
          publishType={publishType}
          options={options}
          isLoadingOptions={isLoadingOptions}
          optionsError={optionsError}
          onKeywordChange={setKeywordInput}
          onRewardTypeChange={(val) => updateSearchParams({ rewardType: val, page: 1 })}
          onValidityTypeChange={(val) => updateSearchParams({ validityType: val, page: 1 })}
          onPublishTypeChange={(val) => updateSearchParams({ publishType: val, page: 1 })}
          onRetryOptions={() => setOptionsRetryKey((k) => k + 1)}
          onClearFilters={handleClearFilters}
          t={t}
        />

        <DataTableCard>
          {!showEmptyState ? (
            <>
              <VoucherDefinitionsTable
                items={items}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                onView={handleViewDetail}
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
              filtered={hasActiveFilters}
              onClear={handleClearFilters}
              t={t}
            />
          )}
        </DataTableCard>
      </div>
    </>
  )
}

function EmptyState({ filtered, onClear, t }) {
  return (
    <div className="grid place-items-center px-6 py-16 text-center">
      <div className="grid size-11 place-items-center rounded-full bg-muted text-primary">
        <TicketIcon size={21} />
      </div>
      <h2 className="mt-4 text-sm font-semibold">
        {t(filtered ? 'voucherDefinitions.noResultsTitle' : 'voucherDefinitions.emptyTitle')}
      </h2>
      <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">
        {t(filtered ? 'voucherDefinitions.noResultsDescription' : 'voucherDefinitions.emptyDescription')}
      </p>
      {filtered ? (
        <div className="mt-4 flex gap-2">
          <Button variant="outline" size="sm" onClick={onClear}>
            {t('voucherDefinitions.filters.clearSearch')}
          </Button>
        </div>
      ) : null}
    </div>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { VoucherDefinitionsPage }
