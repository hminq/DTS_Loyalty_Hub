import { PlusIcon, FileTextIcon, CaretLeftIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useSearchParams, useParams } from 'react-router-dom'

import { getNotificationTemplates, toggleTemplateStatus, getNotificationEventTypes } from '../api/notificationsApi'
import { NotificationTemplatesFilters } from '../components/notifications/NotificationTemplatesFilters'
import { NotificationTemplatesTable } from '../components/notifications/NotificationTemplatesTable'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { Card } from '../components/ui/card'
function NotificationTemplatesPage() {
  const { eventTypeCode } = useParams()
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const requestedPageSize = readPositiveInteger(searchParams.get('pageSize'), 20)
  const pageSize = Math.min(requestedPageSize, 100)
  const keyword = searchParams.get('keyword') || ''

  const [keywordInput, setKeywordInput] = useState(keyword)
  const [templates, setTemplates] = useState([])
  const [meta, setMeta] = useState({ page, pageSize, totalItems: 0, totalPages: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [loadError, setLoadError] = useState('')

  const [eventTypes, setEventTypes] = useState([])
  const [isLoadingEventTypes, setIsLoadingEventTypes] = useState(false)

  const hasActiveFilters = Boolean(keyword)

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

    async function loadEventTypes() {
      setIsLoadingEventTypes(true)
      try {
        const result = await getNotificationEventTypes()
        if (isCurrent) setEventTypes(result || [])
      } catch (error) {
        console.error('Failed to load event types', error)
      } finally {
        if (isCurrent) setIsLoadingEventTypes(false)
      }
    }
    
    loadEventTypes()
    return () => { isCurrent = false }
  }, [])

  useEffect(() => {
    let isCurrent = true

    async function loadTemplates() {
      if (templates.length === 0) setIsLoading(true)
      else setIsRefreshing(true)
      setLoadError('')

      try {
        const response = await getNotificationTemplates({ page, pageSize, keyword, eventTypeCode })
        if (!isCurrent) return

        setTemplates(response.data ?? [])
        setMeta(response.meta ?? { page, pageSize, totalItems: 0, totalPages: 0 })
      } catch (error) {
        if (isCurrent) setLoadError(error.message || t('errors.loadNotificationTemplates', 'Failed to load templates'))
      } finally {
        if (isCurrent) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }

    loadTemplates()
    return () => { isCurrent = false }
  }, [keyword, page, pageSize, eventTypeCode])

  async function handleToggleStatus(templateId) {
    try {
      await toggleTemplateStatus(templateId)
      // Optimistically update the list: if activating, deactivate others with same event/channel/lang
      setTemplates((prev) => {
        const toggled = prev.find(t => t.templateId === templateId)
        if (!toggled) return prev
        const activating = !toggled.isActive
        
        return prev.map((t) => {
          if (t.templateId === templateId) {
            return { ...t, isActive: activating }
          }
          if (activating && t.notificationEventTypeId === toggled.notificationEventTypeId && t.channel === toggled.channel && t.language === toggled.language) {
            return { ...t, isActive: false }
          }
          return t
        })
      })
    } catch (error) {
      console.error('Failed to toggle status', error)
    }
  }

  function clearFilters() {
    setKeywordInput('')
    updateSearchParams({ keyword: '', eventTypeCode: '', page: 1 })
  }

  const resultFrom = meta.totalItems === 0 ? 0 : ((meta.page - 1) * meta.pageSize) + 1
  const resultTo = Math.min(meta.page * meta.pageSize, meta.totalItems)
  const showEmptyState = !isLoading && !loadError && templates.length === 0

  return (
    <>
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/notification-templates')}>
          <CaretLeftIcon size={18} />
        </Button>
        <div className="flex-1">
          <PageHeader
            eyebrow={t('notifications.eyebrow', 'Cấu hình')}
            title={t('notifications.templatesForEvent', { defaultValue: 'Mẫu thông báo: {{eventType}}', eventType: eventTypeCode })}
            description={t('notifications.description', 'Manage templates for emails, SMS, and push notifications.')}
            actions={
              <Button size="sm" onClick={() => navigate(`/notification-templates/new?eventTypeCode=${eventTypeCode}`)}>
                <PlusIcon size={15} weight="bold" />
                {t('notifications.create', 'Create template')}
              </Button>
            }
          />
        </div>
      </div>

      {loadError ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {loadError}
        </p>
      ) : null}

      <Card className="mt-5 overflow-visible rounded-xl border-border/80 shadow-none">
        <NotificationTemplatesFilters
          keyword={keywordInput}
          onKeywordChange={setKeywordInput}
          t={t}
        />

        {!showEmptyState ? (
          <>
            <div className="flex items-center justify-between px-4 py-3">
              <p className="text-xs text-muted-foreground">
                {t('notifications.summary', { defaultValue: `Showing ${resultFrom} to ${resultTo} of ${meta.totalItems} results`, from: resultFrom, to: resultTo, total: meta.totalItems })}
              </p>
            </div>
            <NotificationTemplatesTable
              templates={templates}
              isLoading={isLoading}
              isRefreshing={isRefreshing}
              language={i18n.resolvedLanguage}
              onView={(id) => navigate(`/notification-templates/${id}?eventTypeCode=${eventTypeCode}`)}
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
          <EmptyState
            filtered={hasActiveFilters}
            onCreate={() => navigate(`/notification-templates/new?eventTypeCode=${eventTypeCode}`)}
            onClear={clearFilters}
            t={t}
          />
        )}
      </Card>
    </>
  )
}

function EmptyState({ filtered, onCreate, onClear, t }) {
  return (
    <div className="grid place-items-center px-6 py-16 text-center">
      <div className="grid size-11 place-items-center rounded-full bg-muted text-primary">
        <FileTextIcon size={21} />
      </div>
      <h2 className="mt-4 text-sm font-semibold">
        {t(filtered ? 'notifications.noResultsTitle' : 'notifications.emptyTitle', filtered ? 'No templates found' : 'No templates yet')}
      </h2>
      <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">
        {t(filtered ? 'notifications.noResultsDescription' : 'notifications.emptyDescription', filtered ? 'Try adjusting your filters.' : 'Create your first notification template to start engaging customers.')}
      </p>
      <div className="mt-4 flex gap-2">
        {filtered ? <Button variant="outline" size="sm" onClick={onClear}>{t('notifications.clearFilters', 'Clear filters')}</Button> : null}
        {!filtered ? <Button size="sm" onClick={onCreate}>{t('notifications.create', 'Create template')}</Button> : null}
      </div>
    </div>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { NotificationTemplatesPage }
