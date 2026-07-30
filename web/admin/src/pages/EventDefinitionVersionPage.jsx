import { CheckIcon, CopyIcon, FloppyDiskIcon, ProhibitIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import {
  cloneEventDefinitionVersion,
  getEventDefinitionOptions,
  getEventDefinitionVersion,
  publishEventDefinitionVersion,
  retireEventDefinitionVersion,
  updateEventDefinitionVersion,
} from '../api/eventDefinitionsApi'
import { EventDefinitionSchemaBuilder } from '../components/event-definitions/EventDefinitionSchemaBuilder'
import { CloneEventDefinitionDialog } from '../components/event-definitions/CloneEventDefinitionDialog'
import { PublishEventDefinitionDialog } from '../components/event-definitions/PublishEventDefinitionDialog'
import { RetireEventDefinitionDialog } from '../components/event-definitions/RetireEventDefinitionDialog'
import { mapEventDefinitionOptions } from '../components/event-definitions/eventDefinitionOptions'
import { mapVersionToFormState, buildUpdateDraftSchemaPayload } from '../components/event-definitions/eventDefinitionPayloads'
import { validateSchemaForm } from '../components/event-definitions/eventDefinitionValidation'
import { getStatusBadgeVariant } from '../components/event-definitions/eventDefinitionFormatters'
import { PermissionCodes } from '../constants/permissionCodes'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Badge } from '../components/ui/badge'
import { Button } from '../components/ui/button'

export function EventDefinitionVersionPage() {
  const { t } = useTranslation()
  const { eventTypeId, eventTypeVersionId } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()

  const [versionDetail, setVersionDetail] = useState(null)
  const [optionsData, setOptionsData] = useState({})
  const [fields, setFields] = useState([])
  const [targets, setTargets] = useState([])

  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState(null)
  const [notice, setNotice] = useState(location.state?.notice || null)

  const [fieldErrors, setFieldErrors] = useState([])
  const [targetErrors, setTargetErrors] = useState([])

  // Dialog states
  const [showPublishDialog, setShowPublishDialog] = useState(false)
  const [isPublishing, setIsPublishing] = useState(false)
  const [publishError, setPublishError] = useState(null)

  const [showCloneDialog, setShowCloneDialog] = useState(false)
  const [isCloning, setIsCloning] = useState(false)
  const [cloneError, setCloneError] = useState(null)

  const [showRetireDialog, setShowRetireDialog] = useState(false)
  const [isRetiring, setIsRetiring] = useState(false)
  const [retireError, setRetireError] = useState(null)

  const hasUpdatePermission = hasPermission(PermissionCodes.EventDefinitions.Update)
  const options = useMemo(() => mapEventDefinitionOptions(optionsData, t), [optionsData, t])

  const fetchVersionDetail = useCallback(async () => {
    if (!eventTypeId || !eventTypeVersionId) return
    setIsLoading(true)
    setError(null)

    try {
      const data = await getEventDefinitionVersion(eventTypeId, eventTypeVersionId)
      setVersionDetail(data)
      const form = mapVersionToFormState(data)
      setFields(form.fields)
      setTargets(form.targets)
    } catch (err) {
      setError(err?.message || t('errors.unexpected'))
    } finally {
      setIsLoading(false)
    }
  }, [eventTypeId, eventTypeVersionId, t])

  useEffect(() => {
    getEventDefinitionOptions()
      .then((opts) => setOptionsData(opts || {}))
      .catch(() => {})
  }, [])

  useEffect(() => {
    fetchVersionDetail()
  }, [fetchVersionDetail])

  const canEdit = versionDetail?.canEdit && hasUpdatePermission
  const canPublish = versionDetail?.canPublish && hasUpdatePermission
  const canClone = versionDetail?.canClone && hasUpdatePermission
  const canRetire = versionDetail?.canRetire && hasUpdatePermission

  const handleSaveDraft = async () => {
    if (!canEdit) return
    setError(null)
    setNotice(null)

    const schemaVal = validateSchemaForm(fields, targets)
    setFieldErrors(schemaVal.fieldErrors)
    setTargetErrors(schemaVal.targetErrors)

    if (!schemaVal.isValid) return

    setIsSaving(true)
    try {
      const payload = buildUpdateDraftSchemaPayload(fields, targets)
      const updated = await updateEventDefinitionVersion(eventTypeId, eventTypeVersionId, payload)
      setVersionDetail(updated)
      setNotice(t('eventDefinitions.notices.draftSavedSuccess'))
    } catch (err) {
      setError(err?.response?.data?.error?.message || err?.message || t('errors.unexpected'))
    } finally {
      setIsSaving(false)
    }
  }

  const handleConfirmPublish = async () => {
    setIsPublishing(true)
    setPublishError(null)

    try {
      await publishEventDefinitionVersion(eventTypeId, eventTypeVersionId)
      setShowPublishDialog(false)
      setNotice(t('eventDefinitions.notices.publishedSuccess'))
      fetchVersionDetail()
    } catch (err) {
      setPublishError(err?.response?.data?.error?.message || err?.message || t('errors.unexpected'))
    } finally {
      setIsPublishing(false)
    }
  }

  const handleConfirmClone = async () => {
    setIsCloning(true)
    setCloneError(null)

    try {
      const res = await cloneEventDefinitionVersion(eventTypeId, eventTypeVersionId)
      setShowCloneDialog(false)
      if (res?.eventTypeVersionId) {
        navigate(`/event-definitions/${eventTypeId}/versions/${res.eventTypeVersionId}`, {
          state: { notice: t('eventDefinitions.notices.clonedSuccess') },
        })
      } else {
        fetchVersionDetail()
      }
    } catch (err) {
      setCloneError(err?.response?.data?.error?.message || err?.message || t('errors.unexpected'))
    } finally {
      setIsCloning(false)
    }
  }

  const handleConfirmRetireVersion = async () => {
    setIsRetiring(true)
    setRetireError(null)

    try {
      await retireEventDefinitionVersion(eventTypeId, eventTypeVersionId)
      setShowRetireDialog(false)
      setNotice(t('eventDefinitions.notices.versionRetiredSuccess'))
      fetchVersionDetail()
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
          {t('eventDefinitions.loading')}
        </span>
      </div>
    )
  }

  if (error && !versionDetail) {
    return (
      <div className="mt-5 flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
        <p>{error}</p>
        <Button variant="outline" size="sm" onClick={fetchVersionDetail}>
          {t('common.retry')}
        </Button>
      </div>
    )
  }

  const statusVariant = getStatusBadgeVariant(versionDetail?.status)
  const statusLabel = t(`eventDefinitions.versionStatuses.${versionDetail?.status}`, versionDetail?.status)

  const breadcrumbItems = [
    { label: t('eventDefinitions.title'), to: '/event-definitions' },
    { label: versionDetail?.eventTypeCode || eventTypeId, to: `/event-definitions/${eventTypeId}` },
    { label: `v${versionDetail?.version || ''}` },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        breadcrumb={<Breadcrumb items={breadcrumbItems} />}
        title={`${versionDetail?.eventTypeCode || ''} — v${versionDetail?.version || ''}`}
        description={
          <div className="flex items-center gap-2 mt-1">
            <Badge variant={statusVariant}>{statusLabel}</Badge>
            <span className="text-xs text-muted-foreground">
              ID: {versionDetail?.eventTypeVersionId}
            </span>
          </div>
        }
        actions={
          <div className="flex flex-wrap items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate(`/event-definitions/${eventTypeId}`)}
            >
              {t('eventDefinitions.actions.backToDetail')}
            </Button>

            {canEdit && (
              <Button
                variant="default"
                size="sm"
                onClick={handleSaveDraft}
                disabled={isSaving}
                className="gap-1.5"
              >
                <FloppyDiskIcon size={15} weight="bold" />
                {t('eventDefinitions.actions.saveDraft')}
              </Button>
            )}

            {canPublish && (
              <Button
                variant="default"
                size="sm"
                onClick={() => setShowPublishDialog(true)}
                className="gap-1.5 bg-success text-success-foreground shadow-sm transition-all hover:bg-success/90 hover:shadow"
              >
                <CheckIcon size={15} weight="bold" />
                {t('eventDefinitions.actions.publishDraft')}
              </Button>
            )}

            {canClone && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowCloneDialog(true)}
                className="gap-1.5"
              >
                <CopyIcon size={15} weight="bold" />
                {t('eventDefinitions.actions.cloneVersion')}
              </Button>
            )}

            {canRetire && (
              <Button
                variant="destructive"
                size="sm"
                onClick={() => setShowRetireDialog(true)}
                className="gap-1.5"
              >
                <ProhibitIcon size={15} weight="bold" />
                {t('eventDefinitions.actions.retireVersion')}
              </Button>
            )}
          </div>
        }
      />

      {notice && (
        <div className="rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {notice}
        </div>
      )}

      {error && (
        <div className="flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{error}</p>
        </div>
      )}

      <EventDefinitionSchemaBuilder
        fields={fields}
        targets={targets}
        fieldTypeOptions={options.fieldTypeOptions}
        targetKindOptions={options.targetKindOptions}
        onChangeFields={setFields}
        onChangeTargets={setTargets}
        fieldErrors={fieldErrors}
        targetErrors={targetErrors}
        disabled={!canEdit || isSaving}
      />

      <PublishEventDefinitionDialog
        open={showPublishDialog}
        onOpenChange={setShowPublishDialog}
        versionNumber={versionDetail?.version}
        onConfirm={handleConfirmPublish}
        isSubmitting={isPublishing}
        error={publishError}
      />

      <CloneEventDefinitionDialog
        open={showCloneDialog}
        onOpenChange={setShowCloneDialog}
        sourceVersionNumber={versionDetail?.version}
        onConfirm={handleConfirmClone}
        isSubmitting={isCloning}
        error={cloneError}
      />

      <RetireEventDefinitionDialog
        open={showRetireDialog}
        onOpenChange={setShowRetireDialog}
        mode="version"
        targetName={`v${versionDetail?.version}`}
        onConfirm={handleConfirmRetireVersion}
        isSubmitting={isRetiring}
        error={retireError}
      />
    </div>
  )
}
