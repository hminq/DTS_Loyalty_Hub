import { UsersThreeIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useOutletContext, useSearchParams } from 'react-router-dom'

import { getCustomerAccounts, updateCustomerAccountStatus } from '../api/customerAccountsApi'
import { getTierConfigs } from '../api/tiersApi'
import { CustomerAccountsFilters } from '../components/customer-accounts/CustomerAccountsFilters'
import { CustomerAccountStatusDialog } from '../components/customer-accounts/CustomerAccountStatusDialog'
import { CustomerAccountsTable } from '../components/customer-accounts/CustomerAccountsTable'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function CustomerAccountsPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const { hasPermission } = useOutletContext()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const keyword = searchParams.get('keyword') || ''
  const status = searchParams.get('status') || ''
  const tierId = searchParams.get('tierId') || ''

  const [keywordInput, setKeywordInput] = useState(keyword)
  const [accounts, setAccounts] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [loadError, setLoadError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [statusAccount, setStatusAccount] = useState(null)
  const [refreshKey, setRefreshKey] = useState(0)
  const [tierOptions, setTierOptions] = useState([])
  const [isTierLoading, setIsTierLoading] = useState(false)
  const [tierError, setTierError] = useState('')

  const canFilterByTier = hasPermission(PermissionCodes.Tiers.View)
  const canEditAccount = hasPermission(PermissionCodes.CustomerUsers.Update)
  const canUpdateStatus = hasPermission(PermissionCodes.CustomerUsers.Disable)
  const hasActiveFilters = Boolean(keyword || status || tierId)

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

  useEffect(() => setKeywordInput(keyword), [keyword])

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
    if (!canFilterByTier && tierId) {
      updateSearchParams({ tierId: '', page: 1 }, true)
    }
  }, [canFilterByTier, tierId, updateSearchParams])

  useEffect(() => {
    const controller = new AbortController()

    async function loadAccounts() {
      if (accounts.length === 0) setIsLoading(true)
      else setIsRefreshing(true)
      setLoadError('')

      try {
        const response = await getCustomerAccounts(
          { page, pageSize, keyword, status, tierId },
          controller.signal,
        )
        if (controller.signal.aborted) return

        const nextMeta = response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 }
        if (nextMeta.totalPages > 0 && page > nextMeta.totalPages) {
          updateSearchParams({ page: nextMeta.totalPages }, true)
          return
        }

        setAccounts(response.data ?? [])
        setMeta(nextMeta)
      } catch (error) {
        if (!controller.signal.aborted) {
          setLoadError(error.message || t('errors.loadCustomerAccounts'))
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }

    loadAccounts()
    return () => controller.abort()
  }, [keyword, page, pageSize, refreshKey, status, t, tierId, updateSearchParams])

  useEffect(() => {
    if (!canFilterByTier) {
      setTierOptions([])
      setTierError('')
      return undefined
    }

    const controller = new AbortController()
    setIsTierLoading(true)
    setTierError('')

    getTierConfigs(controller.signal)
      .then((tiers) => {
        if (!controller.signal.aborted) {
          setTierOptions((tiers ?? []).map((tier) => ({
            value: tier.tierConfigId,
            label: tier.name,
          })))
        }
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setTierError(error.message || t('errors.loadTierOptions'))
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsTierLoading(false)
      })

    return () => controller.abort()
  }, [canFilterByTier, t])

  function clearFilters() {
    setKeywordInput('')
    updateSearchParams({ keyword: '', status: '', tierId: '', page: 1 })
  }

  async function handleStatusChange(nextStatus) {
    await updateCustomerAccountStatus(statusAccount.customerId, nextStatus)
    setStatusAccount(null)
    setSuccessMessage(t(nextStatus === 'DISABLE'
      ? 'customerAccounts.status.disableSuccess'
      : 'customerAccounts.status.enableSuccess'))
    setRefreshKey((current) => current + 1)
  }

  const showEmptyState = !isLoading && !loadError && accounts.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('customerAccounts.eyebrow')}
        title={t('customerAccounts.title')}
        description={t('customerAccounts.description')}
      />

      {loadError ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{loadError}</p>
          <Button variant="outline" size="sm" onClick={() => setRefreshKey((current) => current + 1)}>
            {t('customerAccounts.retry')}
          </Button>
        </div>
      ) : null}
      {successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </p>
      ) : null}

      <div className="mt-5">
        <CustomerAccountsFilters
          keyword={keywordInput}
          onKeywordChange={setKeywordInput}
          status={status}
          onStatusChange={(value) => updateSearchParams({ status: value, page: 1 })}
          tierId={tierId}
          onTierChange={(value) => updateSearchParams({ tierId: value, page: 1 })}
          canFilterByTier={canFilterByTier}
          tierOptions={tierOptions}
          isTierLoading={isTierLoading}
          tierError={tierError}
          t={t}
        />

        <DataTableCard>
          {!showEmptyState ? (
            <>
              <CustomerAccountsTable
                accounts={accounts}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                language={i18n.resolvedLanguage}
                canEdit={canEditAccount}
                canUpdateStatus={canUpdateStatus}
                onView={(customerId) => navigate(`/customer-accounts/${customerId}`)}
                onEdit={(customerId) => navigate(`/customer-accounts/${customerId}/edit`)}
                onStatusChange={setStatusAccount}
                t={t}
              />
              <ListPagination
                meta={meta}
                onPageChange={(nextPage) => updateSearchParams({ page: nextPage })}
                onPageSizeChange={(nextPageSize) => updateSearchParams({ pageSize: nextPageSize, page: 1 })}
              />
            </>
          ) : (
            <CustomerAccountsEmptyState filtered={hasActiveFilters} onClear={clearFilters} t={t} />
          )}
        </DataTableCard>
      </div>

      <CustomerAccountStatusDialog
        account={statusAccount}
        open={Boolean(statusAccount)}
        onClose={() => setStatusAccount(null)}
        onConfirm={handleStatusChange}
      />
    </>
  )
}

function CustomerAccountsEmptyState({ filtered, onClear, t }) {
  return (
    <div className="grid place-items-center px-6 py-16 text-center">
      <div className="grid size-11 place-items-center rounded-full bg-muted text-primary">
        <UsersThreeIcon size={21} aria-hidden="true" />
      </div>
      <h2 className="mt-4 text-sm font-semibold">
        {t(filtered ? 'customerAccounts.noResultsTitle' : 'customerAccounts.emptyTitle')}
      </h2>
      <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">
        {t(filtered ? 'customerAccounts.noResultsDescription' : 'customerAccounts.emptyDescription')}
      </p>
      {filtered ? (
        <Button className="mt-4" variant="outline" size="sm" onClick={onClear}>
          {t('customerAccounts.clearFilters')}
        </Button>
      ) : null}
    </div>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { CustomerAccountsPage }
