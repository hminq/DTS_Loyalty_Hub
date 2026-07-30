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
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'

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
      <div className="mt-5 flex h-48 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
        <span className="inline-flex items-center gap-2">
          <CircleNotchIcon className="animate-spin" size={18} aria-hidden="true" />
          {t('eventDefinitions.loadingDetail')}
        </span>
      </div>
    )
  }

  if (error || !detail) {
    return (
      <div className="mt-5 flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
        <p>{error || t('eventDefinitions.errors.notFound')}</p>
        <Button variant="outline" size="sm" onClick={fetchDetail}>
          {t('common.retry')}
        </Button>
      </div>
    )
  }

  const breadcrumbItems = [
    { label: t('eventDefinitions.title'), to: '/event-definitions' },
    { label: detail.name || detail.code || eventTypeId },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        breadcrumb={<Breadcrumb items={breadcrumbItems} />}
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
        <div className="rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
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
