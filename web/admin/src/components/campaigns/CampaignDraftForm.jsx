import { PlusIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { Button } from '../ui/button'
import { CampaignActionCard } from './CampaignActionCard'
import { CampaignMetadataFormFields } from './CampaignMetadataFormFields'

function createDefaultAction() {
  return {
    actionType: 'ISSUE_POINT',
    calculationType: 'FIXED_AMOUNT',
    recipient: 'EVENT_CUSTOMER',
    amount: '50',
    totalCount: '',
    sessionCount: '',
  }
}

function isReferralOnlyCondition(condition) {
  return condition?.sources?.length === 1 && condition.sources[0] === 'REFERRAL'
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
    eventType: '',
    conditionOptionCode: '',
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

  function handleEventTypeChange(nextEventType) {
    const selectedEvent = (options.eventTypes || []).find((e) => e.value === nextEventType)
    const compatibleConditionOptions = (selectedEvent?.conditionOptions || []).map((option) => option.value)

    setFormValues((prev) => {
      const nextConditionOptionCode = compatibleConditionOptions.includes(prev.conditionOptionCode)
        ? prev.conditionOptionCode
        : ''
      const nextCondition = (selectedEvent?.conditionOptions || []).find(
        (option) => option.value === nextConditionOptionCode,
      )
      const isReferralOnly = isReferralOnlyCondition(nextCondition)

      const nextActions =
        isReferralOnly
          ? prev.actions
          : prev.actions.map((action) =>
              action.recipient === 'REFERRER' ? { ...action, recipient: '' } : action,
            )

      return {
        ...prev,
        eventType: nextEventType,
        conditionOptionCode: nextConditionOptionCode,
        actions: nextActions,
      }
    })
  }

  function handleConditionOptionChange(nextConditionOptionCode) {
    setFormValues((prev) => {
      const selectedEvent = (options.eventTypes || []).find((event) => event.value === prev.eventType)
      const nextCondition = (selectedEvent?.conditionOptions || []).find(
        (option) => option.value === nextConditionOptionCode,
      )
      const nextActions =
        isReferralOnlyCondition(nextCondition)
          ? prev.actions
          : prev.actions.map((action) =>
              action.recipient === 'REFERRER' ? { ...action, recipient: '' } : action,
            )

      return {
        ...prev,
        conditionOptionCode: nextConditionOptionCode,
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

  const selectedEvent = (options.eventTypes || []).find((e) => e.value === formValues.eventType)
  const conditionOptions = selectedEvent?.conditionOptions || []
  const selectedCondition = conditionOptions.find(
    (option) => option.value === formValues.conditionOptionCode,
  )

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
        handleConditionOptionChange={handleConditionOptionChange}
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
            disabled={isSubmitting}
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
            eventType={formValues.eventType}
            isReferralOnly={isReferralOnlyCondition(selectedCondition)}
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
