import { CircleNotchIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import {
  cloneEventDefinitionVersion,
  getEventDefinition,
  retireEventDefinition,
} from '../api/eventDefinitionsApi'
import { EventDefinitionDetails } from '../components/event-definitions/EventDefinitionDetails'
import { EventDefinitionLifecycleActions } from '../components/event-definitions/EventDefinitionLifecycleActions'
import { EventDefinitionVersionHistory } from '../components/event-definitions/EventDefinitionVersionHistory'
import { CloneEventDefinitionDialog } from '../components/event-definitions/CloneEventDefinitionDialog'
import { RetireEventDefinitionDialog } from '../components/event-definitions/RetireEventDefinitionDialog'
import { PermissionCodes } from '../constants/permissionCodes'
import { PageHeader } from '../components/layout/PageHeader'

export function EventDefinitionDetailPage() {
  const { t } = useTranslation()
  const { eventTypeId } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()

  const [detail, setDetail] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  const [notice, setNotice] = useState(location.state?.notice || null)

  const [cloneTargetVersion, setCloneTargetVersion] = useState(null)
  const [isCloning, setIsCloning] = useState(false)
  const [cloneError, setCloneError] = useState(null)

  const [showRetireDialog, setShowRetireDialog] = useState(false)
  const [isRetiring, setIsRetiring] = useState(false)
  const [retireError, setRetireError] = useState(null)

  const hasUpdatePermission = hasPermission(PermissionCodes.EventDefinitions.Update)

  const fetchDetail = useCallback(async () => {
    if (!eventTypeId) return
    setIsLoading(true)
    setError(null)

    try {
      const data = await getEventDefinition(eventTypeId)
      setDetail(data)
    } catch (err) {
      setError(err?.message || t('errors.unexpected'))
    } finally {
      setIsLoading(false)
    }
  }, [eventTypeId, t])

  useEffect(() => {
    fetchDetail()
  }, [fetchDetail])

  const handleConfirmClone = async () => {
    if (!cloneTargetVersion) return
    setIsCloning(true)
    setCloneError(null)

    try {
      const res = await cloneEventDefinitionVersion(eventTypeId, cloneTargetVersion.eventTypeVersionId)
      setCloneTargetVersion(null)
      if (res?.eventTypeVersionId) {
        navigate(`/event-definitions/${eventTypeId}/versions/${res.eventTypeVersionId}`, {
          state: { notice: t('eventDefinitions.notices.clonedSuccess') },
        })
      } else {
        fetchDetail()
      }
    } catch (err) {
      setCloneError(err?.response?.data?.error?.message || err?.message || t('errors.unexpected'))
    } finally {
      setIsCloning(false)
    }
  }

  const handleConfirmRetireType = async () => {
    setIsRetiring(true)
    setRetireError(null)

    try {
      await retireEventDefinition(eventTypeId)
      setShowRetireDialog(false)
      setNotice(t('eventDefinitions.notices.typeRetiredSuccess'))
      fetchDetail()
    } catch (err) {
      setRetireError(err?.response?.data?.error?.message || err?.message || t('errors.unexpected'))
    } finally {
      setIsRetiring(false)
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-12 text-muted-foreground">
        <CircleNotchIcon className="mr-2 animate-spin" size={20} />
        {t('eventDefinitions.loadingDetail')}
      </div>
    )
  }

  if (error || !detail) {
    return (
      <div className="rounded-md bg-destructive/10 p-4 text-sm font-medium text-destructive">
        {error || t('eventDefinitions.errors.notFound')}
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={detail.name}
        description={detail.code}
        actions={
          <EventDefinitionLifecycleActions
            detail={detail}
            hasUpdatePermission={hasUpdatePermission}
            onRetireType={() => setShowRetireDialog(true)}
          />
        }
      />

      {notice && (
        <div className="rounded-md bg-emerald-500/10 p-4 text-sm font-medium text-emerald-600">
          {notice}
        </div>
      )}

      <EventDefinitionDetails detail={detail} />

      <EventDefinitionVersionHistory
        eventTypeId={eventTypeId}
        versions={detail.versions || []}
        canCreateDraft={detail.canCreateDraft}
        hasUpdatePermission={hasUpdatePermission}
        onCloneVersion={(ver) => setCloneTargetVersion(ver)}
      />

      <CloneEventDefinitionDialog
        open={Boolean(cloneTargetVersion)}
        onOpenChange={(open) => !open && setCloneTargetVersion(null)}
        sourceVersionNumber={cloneTargetVersion?.version}
        onConfirm={handleConfirmClone}
        isSubmitting={isCloning}
        error={cloneError}
      />

      <RetireEventDefinitionDialog
        open={showRetireDialog}
        onOpenChange={setShowRetireDialog}
        mode="type"
        targetName={detail.name}
        onConfirm={handleConfirmRetireType}
        isSubmitting={isRetiring}
        error={retireError}
      />
    </div>
  )
}
