import { PlusIcon, UserPlusIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useOutletContext, useSearchParams } from 'react-router-dom'

import { getAdminAccounts, updateAdminAccountStatus } from '../api/adminAccountsApi'
import { AdminAccountStatusDialog } from '../components/admin-accounts/AdminAccountStatusDialog'
import { AdminAccountsFilters } from '../components/admin-accounts/AdminAccountsFilters'
import { AdminAccountsTable } from '../components/admin-accounts/AdminAccountsTable'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function AdminAccountsPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const { hasPermission } = useOutletContext()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const requestedPageSize = readPositiveInteger(searchParams.get('pageSize'), 20)
  const pageSize = Math.min(requestedPageSize, 100)
  const keyword = searchParams.get('keyword') || ''
  const status = searchParams.get('status') || ''
  const roleId = searchParams.get('roleId') || ''

  const [keywordInput, setKeywordInput] = useState(keyword)
  const [accounts, setAccounts] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [loadError, setLoadError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [statusAccount, setStatusAccount] = useState(null)
  const [refreshKey, setRefreshKey] = useState(0)

  const canViewRoles = hasPermission(PermissionCodes.Roles.View)
  const canCreateAccount = hasPermission(PermissionCodes.AdminUsers.Create) && canViewRoles
  const canEditAccount = hasPermission(PermissionCodes.AdminUsers.Update) && canViewRoles
  const canUpdateStatus = hasPermission(PermissionCodes.AdminUsers.Disable)
  const hasActiveFilters = Boolean(keyword || status || roleId)

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
    if (searchParams.get('page') !== String(page) || searchParams.get('pageSize') !== String(pageSize)) {
      updateSearchParams({ page, pageSize })
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

    async function loadAccounts() {
      if (accounts.length === 0) setIsLoading(true)
      else setIsRefreshing(true)
      setLoadError('')

      try {
        const response = await getAdminAccounts(
          { page, pageSize, keyword, status, roleId },
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
        if (!controller.signal.aborted) setLoadError(error.message || t('errors.loadAdminAccounts'))
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }

    loadAccounts()
    return () => controller.abort()
  }, [keyword, page, pageSize, refreshKey, roleId, status, t, updateSearchParams])

  function clearFilters() {
    setKeywordInput('')
    updateSearchParams({ keyword: '', status: '', roleId: '', page: 1 })
  }

  async function handleStatusChange(nextStatus) {
    await updateAdminAccountStatus(statusAccount.adminId, nextStatus)
    setStatusAccount(null)
    setSuccessMessage(t(nextStatus === 'DISABLE'
      ? 'adminAccounts.status.disableSuccess'
      : 'adminAccounts.status.enableSuccess'))
    setRefreshKey((current) => current + 1)
  }

  const showEmptyState = !isLoading && !loadError && accounts.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('adminAccounts.eyebrow')}
        title={t('adminAccounts.title')}
        description={t('adminAccounts.description')}
        actions={canCreateAccount ? (
          <Button size="sm" onClick={() => navigate('/admin-accounts/new')}>
            <PlusIcon size={15} weight="bold" />
            {t('adminAccounts.create')}
          </Button>
        ) : null}
      />

      {loadError ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {loadError}
        </p>
      ) : null}
      {successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </p>
      ) : null}

      <div className="mt-5">
        <AdminAccountsFilters
          keyword={keywordInput}
          onKeywordChange={setKeywordInput}
          status={status}
          onStatusChange={(value) => updateSearchParams({ status: value, page: 1 })}
          roleId={roleId}
          onRoleChange={(value) => updateSearchParams({ roleId: value, page: 1 })}
          canFilterByRole={canViewRoles}
          t={t}
        />

        <DataTableCard>
          {!showEmptyState ? (
            <>
              <AdminAccountsTable
                accounts={accounts}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                language={i18n.resolvedLanguage}
                capabilities={{
                  canView: true,
                  canEdit: canEditAccount,
                  canUpdateStatus,
                }}
                onView={(adminId) => navigate(`/admin-accounts/${adminId}`)}
                onEdit={(adminId) => navigate(`/admin-accounts/${adminId}/edit`)}
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
            <EmptyState
              filtered={hasActiveFilters}
              canCreate={canCreateAccount}
              onCreate={() => navigate('/admin-accounts/new')}
              onClear={clearFilters}
              t={t}
            />
          )}
        </DataTableCard>
      </div>

      <AdminAccountStatusDialog
        account={statusAccount}
        open={Boolean(statusAccount)}
        onClose={() => setStatusAccount(null)}
        onConfirm={handleStatusChange}
      />

    </>
  )
}

function EmptyState({ filtered, canCreate, onCreate, onClear, t }) {
  return (
    <div className="grid place-items-center px-6 py-16 text-center">
      <div className="grid size-11 place-items-center rounded-full bg-muted text-primary">
        <UserPlusIcon size={21} />
      </div>
      <h2 className="mt-4 text-sm font-semibold">
        {t(filtered ? 'adminAccounts.noResultsTitle' : 'adminAccounts.emptyTitle')}
      </h2>
      <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">
        {t(filtered ? 'adminAccounts.noResultsDescription' : 'adminAccounts.emptyDescription')}
      </p>
      <div className="mt-4 flex gap-2">
        {filtered ? <Button variant="outline" size="sm" onClick={onClear}>{t('adminAccounts.clearFilters')}</Button> : null}
        {!filtered && canCreate ? <Button size="sm" onClick={onCreate}>{t('adminAccounts.create')}</Button> : null}
      </div>
    </div>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { AdminAccountsPage }
