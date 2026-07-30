import { PlusIcon, FileTextIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useSearchParams } from 'react-router-dom'

import { getNotificationTemplates, toggleTemplateStatus } from '../api/notificationsApi'
import { NotificationTemplatesFilters } from '../components/notifications/NotificationTemplatesFilters'
import { NotificationTemplatesTable } from '../components/notifications/NotificationTemplatesTable'
import { ListPagination } from '../components/data-list/ListPagination'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { Card } from '../components/ui/card'

function NotificationTemplatesPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPositiveInteger(searchParams.get('page'), 1)
  const pageSize = Math.min(readPositiveInteger(searchParams.get('pageSize'), 20), 100)
  const keyword = searchParams.get('keyword') || ''
  const notificationCode = searchParams.get('notificationCode') || ''
  const [keywordInput, setKeywordInput] = useState(keyword)
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

  useEffect(() => setKeywordInput(keyword), [keyword])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      const normalized = keywordInput.trim()
      if (normalized !== keyword) updateSearchParams({ keyword: normalized, page: 1 })
    }, 300)
    return () => window.clearTimeout(timeoutId)
  }, [keyword, keywordInput, updateSearchParams])

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
        if (current) setLoadError(error.message || 'Failed to load notification templates')
      } finally {
        if (current) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      }
    }
    loadTemplates()
    return () => { current = false }
  }, [keyword, notificationCode, page, pageSize])

  async function handleToggleStatus(templateId) {
    try {
      await toggleTemplateStatus(templateId)
      setTemplates((items) => items.map((item) =>
        item.templateId === templateId ? { ...item, isActive: !item.isActive } : item))
    } catch (error) {
      setLoadError(error.message || 'Failed to update template status')
    }
  }

  const hasFilters = Boolean(keyword || notificationCode)
  const resultFrom = meta.totalItems === 0 ? 0 : ((meta.page - 1) * meta.pageSize) + 1
  const resultTo = Math.min(meta.page * meta.pageSize, meta.totalItems)
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

      {loadError ? <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">{loadError}</p> : null}

      <Card className="mt-5 overflow-visible rounded-xl border-border/80 shadow-none">
        <NotificationTemplatesFilters
          keyword={keywordInput}
          onKeywordChange={setKeywordInput}
          notificationCode={notificationCode}
          onNotificationCodeChange={(value) => updateSearchParams({ notificationCode: value, page: 1 })}
          t={t}
        />

        {!empty ? (
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
              onView={(id) => navigate(`/notification-templates/${id}`)}
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
            <div className="grid size-11 place-items-center rounded-full bg-muted text-primary"><FileTextIcon size={21} /></div>
            <h2 className="mt-4 text-sm font-semibold">{t(hasFilters ? 'notifications.noResultsTitle' : 'notifications.emptyTitle', hasFilters ? 'No templates found' : 'No templates yet')}</h2>
            <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">{t(hasFilters ? 'notifications.noResultsDescription' : 'notifications.emptyDescription', 'Create your first notification template.')}</p>
          </div>
        )}
      </Card>
    </>
  )
}

function readPositiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

export { NotificationTemplatesPage }
