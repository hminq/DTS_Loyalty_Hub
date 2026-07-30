import { CircleNotchIcon, PencilSimpleIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useParams } from 'react-router-dom'

import { getNotificationTemplate, toggleTemplateStatus } from '../api/notificationsApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { NotificationTemplateDetails } from '../components/notifications/NotificationTemplateDetails'
import { Button } from '../components/ui/button'

function NotificationTemplateDetailPage() {
  const { id } = useParams()
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()

  const [template, setTemplate] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isTogglingStatus, setIsTogglingStatus] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const returnSearch = location.state?.returnSearch
  const listTarget = returnSearch ? `/notification-templates?${returnSearch}` : '/notification-templates'

  useEffect(() => {
    let current = true
    setIsLoading(true)
    setErrorMessage('')

    getNotificationTemplate(id)
      .then((data) => {
        if (current) setTemplate(data)
      })
      .catch((error) => {
        if (!current) return
        setErrorMessage(error.message || t('notifications.errors.load', 'Failed to load notification template'))
      })
      .finally(() => {
        if (current) setIsLoading(false)
      })

    return () => {
      current = false
    }
  }, [id, refreshKey, t])

  const handleToggleStatus = useCallback(async () => {
    if (!id) return
    setIsTogglingStatus(true)
    setErrorMessage('')
    try {
      await toggleTemplateStatus(id)
      setTemplate((current) => current ? { ...current, isActive: !current.isActive } : null)
    } catch (error) {
      setErrorMessage(error.message || t('notifications.errors.updateStatus', 'Failed to update template status'))
    } finally {
      setIsTogglingStatus(false)
    }
  }, [id, t])

  const actions = (
    <div className="flex items-center gap-2">
      {template ? (
        template.isActive ? (
          <Button
            variant="destructive"
            size="sm"
            onClick={handleToggleStatus}
            disabled={isTogglingStatus}
          >
            {t('notifications.actions.deactivate', 'Deactivate')}
          </Button>
        ) : (
          <Button
            size="sm"
            onClick={handleToggleStatus}
            disabled={isTogglingStatus}
            className="bg-success text-success-foreground shadow-sm transition-all hover:bg-success/90 hover:shadow"
          >
            {t('notifications.actions.activate', 'Activate')}
          </Button>
        )
      ) : null}

      <Button
        variant="outline"
        size="sm"
        onClick={() => navigate(`/notification-templates/${id}/edit`)}
      >
        <PencilSimpleIcon size={15} />
        {t('common.edit', 'Edit')}
      </Button>
    </div>
  )

  return (
    <>
      <PageHeader
        breadcrumb={
          <Breadcrumb
            items={[
              { label: t('notifications.title', 'Notification Templates'), to: listTarget },
              { label: template?.name || t('notifications.detail.titleFallback', 'Template detail') },
            ]}
          />
        }
        title={template?.name || t('notifications.detail.titleFallback', 'Template detail')}
        description={t('notifications.detail.description', 'Manage notification template details and variables.')}
        actions={actions}
      />

      {errorMessage ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button variant="outline" size="sm" onClick={() => setRefreshKey((k) => k + 1)}>
            {t('common.retry', 'Retry')}
          </Button>
        </div>
      ) : null}

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('notifications.loading', 'Loading template...')}
        </div>
      ) : template ? (
        <NotificationTemplateDetails
          template={template}
          language={i18n.resolvedLanguage}
          t={t}
        />
      ) : null}
    </>
  )
}

export { NotificationTemplateDetailPage }
