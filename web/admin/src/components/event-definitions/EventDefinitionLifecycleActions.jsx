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
          <Button variant="outline" size="sm" className="gap-1.5">
            <PencilSimpleIcon size={15} weight="bold" />
            {t('eventDefinitions.actions.editMetadata')}
          </Button>
        </Link>
      )}

      {canRetire && (
        <Button variant="destructive" size="sm" onClick={onRetireType} className="gap-1.5">
          <ProhibitIcon size={15} weight="bold" />
          {t('eventDefinitions.actions.retireType')}
        </Button>
      )}
    </div>
  )
}
