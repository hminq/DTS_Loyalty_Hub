import { GiftIcon, PlusIcon } from '@phosphor-icons/react'
import { useState } from 'react'

import { toFieldErrorMap } from '../../api'
import {
  createCampaignAction,
  updateCampaignAction,
} from '../../api/campaignsApi'
import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { DeleteCampaignActionDialog } from './DeleteCampaignActionDialog'
import { PersistedCampaignActionCard } from './PersistedCampaignActionCard'

export function CampaignActionManager({
  campaignId,
  actions = [],
  isDraft = true,
  canEdit = true,
  options = {},
  eventType = '',
  conditionPresetCode = '',
  onActionsChanged,
  language,
  t,
}) {
  const [isAddingNew, setIsAddingNew] = useState(false)
  const [cardErrors, setCardErrors] = useState({})
  const [cardFieldErrors, setCardFieldErrors] = useState({})
  const [globalError, setGlobalError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [actionToDelete, setActionToDelete] = useState(null)
  const hasCampaignContext = Boolean(eventType && conditionPresetCode)

  const orderedActions = [...(actions || [])].sort(
    (a, b) => (a.executeOrder ?? 0) - (b.executeOrder ?? 0),
  )

  function clearMessages() {
    setGlobalError('')
    setSuccessMessage('')
    setCardErrors({})
    setCardFieldErrors({})
  }

  async function handleSaveAction(actionId, payload) {
    clearMessages()
    try {
      if (actionId) {
        await updateCampaignAction(campaignId, actionId, payload)
        setSuccessMessage(
          t('campaigns.actions.updateSuccess', {
            defaultValue: 'Reward action updated successfully.',
          }),
        )
      } else {
        await createCampaignAction(campaignId, payload)
        setIsAddingNew(false)
        setSuccessMessage(
          t('campaigns.actions.createSuccess', {
            defaultValue: 'Reward action added successfully.',
          }),
        )
      }
      if (onActionsChanged) {
        onActionsChanged()
      }
    } catch (error) {
      const mapped = toFieldErrorMap(error.details)
      const hasFieldErrors = Object.keys(mapped).length > 0
      const targetKey = actionId || 'NEW'
      if (hasFieldErrors) {
        setCardFieldErrors((prev) => ({ ...prev, [targetKey]: mapped }))
      } else {
        const msg =
          error.message ||
          t('campaigns.actions.saveFailed', { defaultValue: 'Failed to save action.' })
        setCardErrors((prev) => ({ ...prev, [targetKey]: msg }))
      }
      throw error
    }
  }

  function handleOpenDelete(action) {
    clearMessages()
    setActionToDelete(action)
    setDeleteDialogOpen(true)
  }

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-base font-semibold text-foreground">
            {t('campaigns.form.actionsTitle', { defaultValue: 'Reward actions' })}
          </h3>
          <p className="mt-0.5 text-xs text-muted-foreground">
            {t('campaigns.form.actionsHelper', {
              defaultValue:
                'Configure one or more reward actions. Execution order follows card order.',
            })}
          </p>
        </div>
        {isDraft && canEdit && !isAddingNew ? (
          <Button
            type="button"
            variant="outline"
            size="sm"
            className="gap-1.5"
            onClick={() => {
              clearMessages()
              setIsAddingNew(true)
            }}
            disabled={!hasCampaignContext}
          >
            <PlusIcon size={14} weight="bold" />
            {t('campaigns.form.addAction', { defaultValue: 'Add action' })}
          </Button>
        ) : null}
      </div>

      {successMessage ? (
        <div className="rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </div>
      ) : null}

      {globalError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {globalError}
        </div>
      ) : null}

      {orderedActions.length === 0 && !isAddingNew ? (
        <Card>
          <CardContent className="py-12">
            <div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
              <GiftIcon size={32} className="opacity-50" />
              <p className="text-sm font-medium">
                {t('campaigns.detail.noActions', {
                  defaultValue: 'No reward actions configured for this campaign.',
                })}
              </p>
              {isDraft && canEdit ? (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="mt-2 gap-1.5"
                  onClick={() => setIsAddingNew(true)}
                  disabled={!hasCampaignContext}
                >
                  <PlusIcon size={14} weight="bold" />
                  {t('campaigns.form.addAction', { defaultValue: 'Add action' })}
                </Button>
              ) : null}
            </div>
          </CardContent>
        </Card>
      ) : null}

      {orderedActions.map((action, index) => (
        <PersistedCampaignActionCard
          key={action.actionId || index}
          action={action}
          isNew={false}
          isDraft={isDraft}
          canEdit={canEdit}
          options={options}
          eventType={eventType}
          conditionPresetCode={conditionPresetCode}
          externalError={cardErrors[action.actionId] || ''}
          externalFieldErrors={cardFieldErrors[action.actionId] || {}}
          onSave={handleSaveAction}
          onDelete={handleOpenDelete}
          language={language}
          t={t}
        />
      ))}

      {isAddingNew ? (
        <PersistedCampaignActionCard
          key="new-action-card"
          action={{
            actionType: '',
            actionConfig: {
              target: { selector: '' },
              parameters: {},
            },
            executeOrder: orderedActions.length + 1,
            totalCount: null,
            sessionCount: null,
          }}
          isNew={true}
          isDraft={isDraft}
          canEdit={canEdit}
          options={options}
          eventType={eventType}
          conditionPresetCode={conditionPresetCode}
          externalError={cardErrors.NEW || ''}
          externalFieldErrors={cardFieldErrors.NEW || {}}
          onSave={handleSaveAction}
          onCancelNew={() => {
            setIsAddingNew(false)
            setCardErrors((prev) => {
              const next = { ...prev }
              delete next.NEW
              return next
            })
            setCardFieldErrors((prev) => {
              const next = { ...prev }
              delete next.NEW
              return next
            })
          }}
          language={language}
          t={t}
        />
      ) : null}

      <DeleteCampaignActionDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        campaignId={campaignId}
        action={actionToDelete}
        onSuccess={() => {
          setSuccessMessage(
            t('campaigns.actions.deleteSuccess', {
              defaultValue: 'Reward action deleted successfully.',
            }),
          )
          if (onActionsChanged) {
            onActionsChanged()
          }
        }}
        onNotDraft={(msg) => setGlobalError(msg)}
        onLastAction={(msg) => setGlobalError(msg)}
        t={t}
      />
    </div>
  )
}
