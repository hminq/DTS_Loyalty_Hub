import { ClockCounterClockwiseIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useSearchParams } from 'react-router-dom'

import { getCustomerRedeems } from '../api/customerVouchersApi'
import { CustomerRedeemsTable } from '../components/customer-vouchers/CustomerRedeemsTable'
import {
  CustomerVoucherHistoryFilters,
  hasCustomerVoucherFilters,
} from '../components/customer-vouchers/CustomerVoucherHistoryFilters'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { EmptyState } from '../components/data-list/EmptyState'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'

function VoucherRedemptionsPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const filters = useMemo(() => ({
    voucherKeyword: searchParams.get('voucherKeyword') || '',
    userKeyword: searchParams.get('userKeyword') || '',
    campaignName: searchParams.get('campaignName') || '',
    rewardType: searchParams.get('rewardType') || '',
    redeemAtFrom: searchParams.get('redeemAtFrom') || '',
    redeemAtTo: searchParams.get('redeemAtTo') || '',
  }), [searchParams])

  const [items, setItems] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)
  const hasLoadedRef = useRef(false)

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
    if (
      searchParams.get('page') !== String(page)
      || searchParams.get('pageSize') !== String(pageSize)
    ) {
      updateSearchParams({ page, pageSize }, true)
    }
  }, [page, pageSize, searchParams, updateSearchParams])

  useEffect(() => {
    const controller = new AbortController()

    if (hasLoadedRef.current) setIsRefreshing(true)
    else setIsLoading(true)
    setErrorMessage('')

    getCustomerRedeems({ page, pageSize, ...filters }, controller.signal)
      .then((response) => {
        if (controller.signal.aborted) return

        const nextMeta = response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 }
        setItems(response.data ?? [])
        setMeta(nextMeta)
        hasLoadedRef.current = true

        if (nextMeta.totalPages > 0 && page > nextMeta.totalPages) {
          updateSearchParams({ page: nextMeta.totalPages }, true)
        }
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setErrorMessage(error.message || t('voucherRedemptions.errors.loadList'))
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      })

    return () => controller.abort()
  }, [filters, page, pageSize, refreshKey, t, updateSearchParams])

  function applyFilters(nextFilters) {
    updateSearchParams({ ...nextFilters, page: 1 })
  }

  function clearFilters() {
    updateSearchParams({
      voucherKeyword: '',
      userKeyword: '',
      campaignName: '',
      rewardType: '',
      redeemAtFrom: '',
      redeemAtTo: '',
      page: 1,
    })
  }

  function viewRedemption(voucherRedemptionId) {
    navigate(`/vouchers/redemptions/${voucherRedemptionId}`, {
      state: { returnSearch: searchParams.toString() },
    })
  }

  const filtered = hasCustomerVoucherFilters(filters, true)
  const showEmptyState = !isLoading && items.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('voucherRedemptions.eyebrow')}
        title={t('voucherRedemptions.title')}
        description={t('voucherRedemptions.description')}
      />

      {errorMessage ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button variant="outline" size="sm" onClick={() => setRefreshKey((key) => key + 1)}>
            {t('common.retry')}
          </Button>
        </div>
      ) : null}

      <div className="mt-5">
        <CustomerVoucherHistoryFilters
          filters={filters}
          includeCampaign
          onApply={applyFilters}
          onClear={clearFilters}
          presentation="popover"
        />

        <DataTableCard>
          {showEmptyState ? (
            <EmptyState
              icon={ClockCounterClockwiseIcon}
              title={t(filtered ? 'voucherRedemptions.noResultsTitle' : 'voucherRedemptions.emptyTitle')}
              description={t(filtered ? 'voucherRedemptions.noResultsDescription' : 'voucherRedemptions.emptyDescription')}
              filtered={filtered}
              onClearSearch={clearFilters}
              t={t}
            />
          ) : (
            <>
              <CustomerRedeemsTable
                items={items}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                language={i18n.resolvedLanguage}
                t={t}
                onView={viewRedemption}
              />
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

export { VoucherRedemptionsPage }
