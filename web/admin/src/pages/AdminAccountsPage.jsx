import { PlusIcon, UserPlusIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useOutletContext, useSearchParams } from 'react-router-dom'

import { getAdminAccounts } from '../api/adminAccountsApi'
import { AdminAccountsFilters } from '../components/admin-accounts/AdminAccountsFilters'
import { AdminAccountsTable } from '../components/admin-accounts/AdminAccountsTable'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { Card } from '../components/ui/card'
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

  const canViewRoles = hasPermission(PermissionCodes.Roles.View)
  const canCreateAccount = hasPermission(PermissionCodes.AdminUsers.Create) && canViewRoles
  const hasActiveFilters = Boolean(keyword || status || roleId)

  const updateSearchParams = useCallback((updates) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)

      Object.entries(updates).forEach(([key, value]) => {
        if (value === '' || value === null || value === undefined) next.delete(key)
        else next.set(key, String(value))
      })

      if (!next.has('page')) next.set('page', '1')
      if (!next.has('pageSize')) next.set('pageSize', String(pageSize))
      return next
    })
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
    let isCurrent = true

    async function loadAccounts() {
      if (accounts.length === 0) setIsLoading(true)
      else setIsRefreshing(true)
      setLoadError('')

      try {
        const response = await getAdminAccounts({ page, pageSize, keyword, status, roleId })
        if (!isCurrent) return

        setAccounts(response.data ?? [])
        setMeta(response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 })
      } catch (error) {
        if (isCurrent) setLoadError(error.message || t('errors.loadAdminAccounts'))
      } finally {
        if (isCurrent) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }

    loadAccounts()
    return () => { isCurrent = false }
  }, [keyword, page, pageSize, roleId, status])

  function clearFilters() {
    setKeywordInput('')
    updateSearchParams({ keyword: '', status: '', roleId: '', page: 1 })
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

      <Card className="mt-5 overflow-visible rounded-xl border-border/80 shadow-none">
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

        {!showEmptyState ? (
          <>
            <AdminAccountsTable
              accounts={accounts}
              isLoading={isLoading}
              isRefreshing={isRefreshing}
              language={i18n.resolvedLanguage}
              onView={(adminId) => navigate(`/admin-accounts/${adminId}`)}
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
      </Card>

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
