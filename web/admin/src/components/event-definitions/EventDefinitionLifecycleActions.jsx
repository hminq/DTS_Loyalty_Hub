import { PencilSimpleIcon, ProhibitIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { Button } from '../ui/button'

export function EventDefinitionLifecycleActions({
  detail = {},
  hasUpdatePermission = false,
  onRetireType,
}) {
  const { t } = useTranslation()

  const isRetired = detail.status === 'RETIRED'
  const canEdit = !isRetired && hasUpdatePermission
  const canRetire = detail.canRetire && !isRetired && hasUpdatePermission

  if (!canEdit && !canRetire) return null

  return (
    <div className="flex items-center gap-2">
      {canEdit && (
        <Link to={`/event-definitions/${detail.eventTypeId}/edit`}>
          <Button variant="outline">
            <PencilSimpleIcon className="mr-1.5 h-4 w-4" />
            {t('eventDefinitions.actions.editMetadata')}
          </Button>
        </Link>
      )}

      {canRetire && (
        <Button variant="destructive" onClick={onRetireType}>
          <ProhibitIcon className="mr-1.5 h-4 w-4" />
          {t('eventDefinitions.actions.retireType')}
        </Button>
      )}
    </div>
  )
}
