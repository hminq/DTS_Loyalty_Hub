import { TicketIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useSearchParams } from 'react-router-dom'

import { getAllCustomerVouchers } from '../api/customerVouchersApi'
import {
  CustomerVoucherHistoryFilters,
  hasCustomerVoucherFilters,
} from '../components/customer-vouchers/CustomerVoucherHistoryFilters'
import { CustomerVouchersTable } from '../components/customer-vouchers/CustomerVouchersTable'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { EmptyState } from '../components/data-list/EmptyState'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'

function AllCustomerVouchersPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const filters = useMemo(() => ({
    voucherKeyword: searchParams.get('voucherKeyword') || '',
    userKeyword: searchParams.get('userKeyword') || '',
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

    getAllCustomerVouchers({ page, pageSize, ...filters }, controller.signal)
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
          setErrorMessage(error.message || t('customerVouchers.errors.loadList'))
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
      rewardType: '',
      redeemAtFrom: '',
      redeemAtTo: '',
      page: 1,
    })
  }

  const filtered = hasCustomerVoucherFilters(filters)
  const showEmptyState = !isLoading && items.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('customerVouchers.eyebrow')}
        title={t('customerVouchers.title')}
        description={t('customerVouchers.description')}
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
          onApply={applyFilters}
          onClear={clearFilters}
          presentation="popover"
        />

        <DataTableCard>
          {showEmptyState ? (
            <EmptyState
              icon={TicketIcon}
              title={t(filtered ? 'customerVouchers.noResultsTitle' : 'customerVouchers.emptyTitle')}
              description={t(filtered ? 'customerVouchers.noResultsDescription' : 'customerVouchers.emptyDescription')}
              filtered={filtered}
              onClearSearch={clearFilters}
              t={t}
            />
          ) : (
            <>
              <CustomerVouchersTable
                items={items}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                onView={(voucher) => navigate(`/vouchers/customer-vouchers/${voucher.cusVoucherId}`, {
                  state: { returnSearch: searchParams.toString() },
                })}
                t={t}
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

export { AllCustomerVouchersPage }
