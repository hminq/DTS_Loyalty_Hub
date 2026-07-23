import { PlusIcon, ShieldStarIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useSearchParams } from 'react-router-dom'

import { deleteRole, getRoles } from '../api/rolesApi'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { DeleteRoleDialog } from '../components/roles/DeleteRoleDialog'
import { RolesTable } from '../components/roles/RolesTable'
import { EmptyState } from '../components/data-list/EmptyState'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function RolesPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const keyword = searchParams.get('keyword') || ''
  const [roles, setRoles] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [routeErrorMessage, setRouteErrorMessage] = useState(location.state?.errorMessage ?? '')
  const [successMessage, setSuccessMessage] = useState(location.state?.successMessage ?? '')
  const [roleToDelete, setRoleToDelete] = useState(null)
  const [refreshKey, setRefreshKey] = useState(0)

  const canCreate = hasPermission(PermissionCodes.Roles.Create)
  const canEdit = hasPermission(PermissionCodes.Roles.Update)
  const canDelete = hasPermission(PermissionCodes.Roles.Delete)

  const showEmptyState = !isLoading && roles.length === 0
  const hasActiveFilters = Boolean(keyword)

  const updateSearchParams = useCallback((updates) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)
      Object.entries(updates).forEach(([key, value]) => {
        if (value === '' || value === null || value === undefined) next.delete(key)
        else next.set(key, String(value))
      })
      return next
    })
  }, [setSearchParams])

  useEffect(() => {
    if (location.state?.successMessage || location.state?.errorMessage) {
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    let isCurrent = true

    async function loadRoles() {
      setIsLoading(true)
      setErrorMessage('')

      try {
        const response = await getRoles({ page, pageSize, keyword })
        if (!isCurrent) return

        const nextMeta = response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 }
        if (nextMeta.totalPages > 0 && page > nextMeta.totalPages) {
          updateSearchParams({ page: nextMeta.totalPages })
          return
        }

        setRoles(response.data ?? [])
        setMeta(nextMeta)
      } catch (error) {
        if (isCurrent) setErrorMessage(error.message || t('errors.loadRoles'))
      } finally {
        if (isCurrent) setIsLoading(false)
      }
    }

    loadRoles()
    return () => { isCurrent = false }
  }, [keyword, page, pageSize, refreshKey, t, updateSearchParams])

  async function handleDelete(role) {
    try {
      await deleteRole(role.roleId)
    } catch (error) {
      if (error.code === 'ROLE_NOT_FOUND') {
        setRoleToDelete(null)
        setRouteErrorMessage(error.message)
        setRefreshKey((current) => current + 1)
        return
      }
      throw error
    }

    setRoleToDelete(null)
    setSuccessMessage(t('roles.delete.success', { name: role.name }))

    if (roles.length === 1 && page > 1) {
      updateSearchParams({ page: page - 1 })
    } else {
      setRefreshKey((current) => current + 1)
    }
  }

  return (
    <>
      <PageHeader
        eyebrow={t('roles.eyebrow')}
        title={t('roles.title')}
        description={t('roles.description')}
        actions={canCreate ? (
          <Button size="sm" onClick={() => navigate('/roles/new')}>
            <PlusIcon size={15} weight="bold" />
            {t('roles.create')}
          </Button>
        ) : null}
      />

      {successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success/10 px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </p>
      ) : null}
      {errorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {errorMessage}
        </p>
      ) : null}
      {routeErrorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {routeErrorMessage}
        </p>
      ) : null}

      <DataTableCard className="mt-5">
        {!showEmptyState ? (
          <>
            <RolesTable
              roles={roles}
              isLoading={isLoading}
              language={i18n.resolvedLanguage}
              capabilities={{ canView: true, canEdit, canDelete }}
              onView={(roleId) => navigate(`/roles/${roleId}`)}
              onEdit={(roleId) => navigate(`/roles/${roleId}/edit`)}
              onDelete={setRoleToDelete}
            />
            <ListPagination
              meta={meta}
              onPageChange={(nextPage) => updateSearchParams({ page: nextPage })}
              onPageSizeChange={(nextPageSize) => updateSearchParams({ pageSize: nextPageSize, page: 1 })}
            />
          </>
        ) : (
          <EmptyState
            icon={ShieldStarIcon}
            title={t(hasActiveFilters ? 'roles.noResultsTitle' : 'roles.emptyTitle')}
            description={t(hasActiveFilters ? 'roles.noResultsDescription' : 'roles.emptyDescription')}
            filtered={hasActiveFilters}
            onClearSearch={() => updateSearchParams({ keyword: '', page: 1 })}
            t={t}
          />
        )}
      </DataTableCard>

      <DeleteRoleDialog
        role={roleToDelete}
        onClose={() => setRoleToDelete(null)}
        onConfirm={handleDelete}
      />
    </>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { RolesPage }
