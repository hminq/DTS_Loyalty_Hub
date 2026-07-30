import { PlusIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { Button } from '../ui/button'
import { CampaignActionCard } from './CampaignActionCard'
import { createMatchAllConditionFormState } from './campaignConditions'
import { CampaignMetadataFormFields } from './CampaignMetadataFormFields'

function isTargetCompatible(target, actionType) {
  if (!target || !actionType) return false
  return target.targetKind === actionType.requiredTargetKind
}

function getCompatibleActionTypes(actionTypes = [], eventVersion) {
  if (!eventVersion) return []
  return actionTypes.filter((actionType) =>
    (eventVersion.targets || []).some((target) =>
      isTargetCompatible(target, actionType),
    ),
  )
}

function createDefaultAction() {
  return {
    actionType: '',
    targetSelector: '',
    parameters: {},
    totalCount: '',
    sessionCount: '',
  }
}

function reconcileAction(action, selectedVersion, actionTypes) {
  const compatibleActionTypes = getCompatibleActionTypes(
    actionTypes,
    selectedVersion,
  )
  const actionDefinition = compatibleActionTypes.find(
    (actionType) => actionType.value === action.actionType,
  )

  if (!actionDefinition) {
    return {
      ...action,
      actionType: '',
      targetSelector: '',
      parameters: {},
    }
  }

  const targetDefinition = (selectedVersion?.targets || []).find(
    (target) => target.value === action.targetSelector,
  )
  const targetSelector = isTargetCompatible(
    targetDefinition,
    actionDefinition,
  )
    ? action.targetSelector
    : ''
  const parameters = Object.fromEntries(
    (actionDefinition.parameters || [])
      .filter((parameter) => action.parameters?.[parameter.code] !== undefined)
      .map((parameter) => [parameter.code, action.parameters[parameter.code]]),
  )

  return {
    ...action,
    targetSelector,
    parameters,
  }
}

export function CampaignDraftForm({
  options = {},
  canUploadBanner = false,
  isSubmitting = false,
  formError = '',
  fieldErrors = {},
  onSubmit,
  onCancel,
  t,
}) {
  const [formValues, setFormValues] = useState({
    campaignName: '',
    description: '',
    bannerFile: null,
    bannerImageKey: '',
    bannerImageUrl: '',
    eventTypeVersionId: '',
    ...createMatchAllConditionFormState(),
    startDate: '',
    endDate: '',
    scheduleCron: '0 0 2 * * ?',
    durationHour: '2',
    userLimitTotal: '',
    userLimitSession: '',
    actions: [createDefaultAction()],
  })

  function updateField(field, value) {
    setFormValues((prev) => ({ ...prev, [field]: value }))
  }

  function handleEventTypeChange(nextVersionId) {
    const selectedVersion = (options.eventTypeVersions || []).find((e) => e.value === nextVersionId)

    setFormValues((prev) => {
      const nextActions = prev.actions.map((action) =>
        reconcileAction(action, selectedVersion, options.actionTypes),
      )

      return {
        ...prev,
        eventTypeVersionId: nextVersionId,
        ...createMatchAllConditionFormState(),
        actions: nextActions,
      }
    })
  }

  function handleActionChange(index, nextAction) {
    setFormValues((prev) => ({
      ...prev,
      actions: prev.actions.map((a, i) => (i === index ? nextAction : a)),
    }))
  }

  function handleAddAction() {
    setFormValues((prev) => ({
      ...prev,
      actions: [...prev.actions, createDefaultAction()],
    }))
  }

  function handleRemoveAction(index) {
    setFormValues((prev) => {
      if (prev.actions.length <= 1) return prev
      return {
        ...prev,
        actions: prev.actions.filter((_, i) => i !== index),
      }
    })
  }

  function handleSubmit(event) {
    event.preventDefault()
    if (onSubmit) {
      onSubmit(formValues)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="grid gap-6">
      {formError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {formError}
        </div>
      ) : null}

      <CampaignMetadataFormFields
        formValues={formValues}
        options={options}
        canUploadBanner={canUploadBanner}
        isSubmitting={isSubmitting}
        fieldErrors={fieldErrors}
        updateField={updateField}
        handleEventTypeChange={handleEventTypeChange}
        t={t}
      />

      {/* Actions */}
      <div className="grid gap-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold">
              {t('campaigns.form.actionsTitle', { defaultValue: 'Reward actions' })}
            </h3>
            <p className="mt-0.5 text-xs text-muted-foreground">
              {t('campaigns.form.actionsHelper', {
                defaultValue:
                  'Configure one or more reward actions. Execution order follows card order.',
              })}
            </p>
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            className="gap-1"
            onClick={handleAddAction}
            disabled={
              isSubmitting ||
              !formValues.eventTypeVersionId
            }
          >
            <PlusIcon size={14} weight="bold" />
            {t('campaigns.form.addAction', { defaultValue: 'Add action' })}
          </Button>
        </div>

        {fieldErrors.actions ? (
          <div className="rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
            {fieldErrors.actions}
          </div>
        ) : null}

        {formValues.actions.map((action, index) => (
          <CampaignActionCard
            key={index}
            index={index}
            action={action}
            options={options}
            eventDefinition={(options.eventTypeVersions || []).find(e => e.value === formValues.eventTypeVersionId)}
            fieldErrors={fieldErrors}
            cardError={fieldErrors[`actions[${index}].actionConfig`] || ''}
            isSubmitting={isSubmitting}
            canRemove={formValues.actions.length > 1}
            onChange={handleActionChange}
            onRemove={handleRemoveAction}
            t={t}
          />
        ))}
      </div>

      {/* Submit */}
      <div className="flex items-center justify-end gap-3 border-t border-border pt-6">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          {t('common.cancel')}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? t('campaigns.form.creatingCampaign', { defaultValue: 'Creating campaign...' })
            : t('campaigns.form.createCampaign', { defaultValue: 'Create campaign' })}
        </Button>
      </div>
    </form>
  )
}
