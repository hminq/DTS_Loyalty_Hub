import { FileTextIcon, PlusIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useSearchParams } from 'react-router-dom'

import { getNotificationTemplates, toggleTemplateStatus } from '../api/notificationsApi'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { NotificationTemplatesFilters } from '../components/notifications/NotificationTemplatesFilters'
import { NotificationTemplatesTable } from '../components/notifications/NotificationTemplatesTable'
import { Button } from '../components/ui/button'

function NotificationTemplatesPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const keyword = searchParams.get('keyword') || ''
  const notificationCode = searchParams.get('notificationCode') || ''
  const [templates, setTemplates] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [loadError, setLoadError] = useState('')

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
    let current = true
    async function loadTemplates() {
      if (templates.length === 0) setIsLoading(true)
      else setIsRefreshing(true)
      setLoadError('')
      try {
        const response = await getNotificationTemplates({ page, pageSize, keyword, notificationCode })
        if (!current) return
        setTemplates(response.data ?? [])
        setMeta(response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 })
      } catch (error) {
        if (current) setLoadError(error.message || t('notifications.errors.load', 'Failed to load notification templates'))
      } finally {
        if (current) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }
    loadTemplates()
    return () => { current = false }
  }, [keyword, notificationCode, page, pageSize, t, templates.length])

  async function handleToggleStatus(templateId) {
    try {
      await toggleTemplateStatus(templateId)
      setTemplates((items) => items.map((item) =>
        item.templateId === templateId ? { ...item, isActive: !item.isActive } : item))
    } catch (error) {
      setLoadError(error.message || t('notifications.errors.updateStatus', 'Failed to update template status'))
    }
  }

  function handleApplyFilters(nextFilters) {
    updateSearchParams({ ...nextFilters, page: 1 })
  }

  function handleClearFilters() {
    updateSearchParams({ keyword: '', notificationCode: '', page: 1 })
  }

  const hasFilters = Boolean(keyword || notificationCode)
  const empty = !isLoading && !loadError && templates.length === 0

  return (
    <>
      <PageHeader
        eyebrow={t('notifications.eyebrow', 'Cấu hình')}
        title={t('notifications.templatesTitle', 'Notification templates')}
        description={t('notifications.description', 'Manage notification templates and variables.')}
        actions={
          <Button size="sm" onClick={() => navigate('/notification-templates/new')}>
            <PlusIcon size={15} weight="bold" />
            {t('notifications.create', 'Create template')}
          </Button>
        }
      />

      {loadError ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {loadError}
        </p>
      ) : null}

      <div className="mt-5">
        <NotificationTemplatesFilters
          filters={{ keyword, notificationCode }}
          onApply={handleApplyFilters}
          onClear={handleClearFilters}
          presentation="popover"
        />

        <DataTableCard>
          {!empty ? (
            <>
              <NotificationTemplatesTable
                templates={templates}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                language={i18n.resolvedLanguage}
                onView={(id) => navigate(`/notification-templates/${id}`)}
                onEdit={(id) => navigate(`/notification-templates/${id}/edit`)}
                onToggleStatus={handleToggleStatus}
                t={t}
              />
              <ListPagination
                meta={meta}
                onPageChange={(nextPage) => updateSearchParams({ page: nextPage })}
                onPageSizeChange={(nextPageSize) => updateSearchParams({ pageSize: nextPageSize, page: 1 })}
              />
            </>
          ) : (
            <div className="grid place-items-center px-6 py-16 text-center">
              <div className="grid size-11 place-items-center rounded-full bg-muted text-primary">
                <FileTextIcon size={21} />
              </div>
              <h2 className="mt-4 text-sm font-semibold">
                {t(hasFilters ? 'notifications.noResultsTitle' : 'notifications.emptyTitle', hasFilters ? 'No templates found' : 'No templates yet')}
              </h2>
              <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">
                {t(hasFilters ? 'notifications.noResultsDescription' : 'notifications.emptyDescription', 'Create your first notification template.')}
              </p>
              {hasFilters ? (
                <div className="mt-4 flex gap-2">
                  <Button variant="outline" size="sm" onClick={handleClearFilters}>
                    {t('common.filters.clearAll', 'Clear all filters')}
                  </Button>
                </div>
              ) : null}
            </div>
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

export { NotificationTemplatesPage }
