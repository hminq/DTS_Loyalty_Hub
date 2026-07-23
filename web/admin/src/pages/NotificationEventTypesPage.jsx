import { BellIcon, ArrowRightIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { getNotificationEventTypes } from '../api/notificationsApi'
import { PageHeader } from '../components/layout/PageHeader'
import { Card } from '../components/ui/card'
import { Button } from '../components/ui/button'

export function NotificationEventTypesPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [eventTypes, setEventTypes] = useState([])
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isCurrent = true
    async function loadEventTypes() {
      setIsLoading(true)
      try {
        const result = await getNotificationEventTypes()
        if (isCurrent) setEventTypes(result || [])
      } catch (error) {
        console.error('Failed to load event types', error)
      } finally {
        if (isCurrent) setIsLoading(false)
      }
    }
    loadEventTypes()
    return () => { isCurrent = false }
  }, [])

  return (
    <>
      <PageHeader
        eyebrow={t('notifications.eyebrow', 'Cấu hình')}
        title={t('notifications.eventTypesTitle', 'Loại sự kiện thông báo')}
        description={t('notifications.eventTypesDescription', 'Chọn một loại sự kiện để quản lý các mẫu thông báo tương ứng.')}
      />

      <Card className="mt-5 overflow-visible rounded-xl border-border/80 shadow-none p-4">
        {isLoading ? (
          <div className="p-8 text-center">{t('common.loading', 'Loading...')}</div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {eventTypes.map(type => (
              <div key={type.eventTypeCode} className="border border-gray-200 rounded-lg p-5 hover:border-blue-500 hover:shadow-sm transition bg-white flex flex-col">
                <div className="flex items-center gap-3 mb-3">
                  <div className="p-2 bg-blue-50 rounded-lg">
                    <BellIcon size={24} className="text-blue-600" />
                  </div>
                  <h3 className="font-semibold text-gray-900">{type.displayName}</h3>
                </div>
                <p className="text-sm text-gray-500 mb-5 flex-grow">
                  {type.description}
                </p>
                <div className="flex justify-end mt-auto">
                  <Button 
                    variant="outline" 
                    className="w-full justify-center gap-2"
                    onClick={() => navigate(`/notification-templates/events/${type.eventTypeCode}`)}
                  >
                    {t('notifications.actions.manageTemplates', 'Quản lý mẫu')}
                    <ArrowRightIcon size={16} />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>
    </>
  )
}
