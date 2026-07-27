import { useState } from 'react'

import { deleteCampaignAction } from '../../api/campaignsApi'
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '../ui/alert-dialog'
import { Button } from '../ui/button'

export function DeleteCampaignActionDialog({
  open,
  onOpenChange,
  campaignId,
  action,
  onSuccess,
  onNotDraft,
  onLastAction,
  t,
}) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  if (!action || !campaignId) return null

  const actionTypeLabel = t(`campaigns.actionTypes.${action.actionType}`, {
    defaultValue: action.actionType || 'Action',
  })

  async function handleDelete() {
    setIsDeleting(true)
    setErrorMessage('')

    try {
      await deleteCampaignAction(campaignId, action.actionId || action.campaignActionId)
      onOpenChange(false)
      if (onSuccess) {
        onSuccess()
      }
    } catch (error) {
      const code = error.code || ''
      if (code === 'CAMPAIGN_NOT_DRAFT') {
        if (onNotDraft) {
          onNotDraft(
            error.message ||
              t('campaigns.errors.notDraft', {
                defaultValue:
                  'Only draft campaigns can be modified. This campaign is read-only.',
              }),
          )
        }
        return
      }
      if (code === 'CAMPAIGN_LAST_ACTION') {
        if (onLastAction) {
          onLastAction(
            error.message ||
              t('campaigns.errors.lastAction', {
                defaultValue:
                  'A draft campaign must have at least one reward action.',
              }),
          )
        }
        return
      }
      setErrorMessage(
        error.message ||
          t('campaigns.errors.deleteActionFailed', {
            defaultValue: 'Failed to delete reward action.',
          }),
      )
    } finally {
      setIsDeleting(false)
    }
  }

  return (
    <AlertDialog open={open} onOpenChange={(val) => (!isDeleting ? onOpenChange(val) : null)}>
      <AlertDialogContent>
        <AlertDialogTitle>
          {t('campaigns.actions.deleteTitle', { defaultValue: 'Delete reward action' })}
        </AlertDialogTitle>
        <AlertDialogDescription>
          {t('campaigns.actions.deleteDescription', {
            actionType: actionTypeLabel,
            order: action.executeOrder ?? 1,
            defaultValue: `Are you sure you want to delete reward action "${actionTypeLabel}" (order #${action.executeOrder ?? 1})? This action cannot be undone.`,
          })}
        </AlertDialogDescription>

        {errorMessage ? (
          <div className="mt-3 rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
            {errorMessage}
          </div>
        ) : null}

        <AlertDialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isDeleting}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="button"
            variant="destructive"
            onClick={handleDelete}
            disabled={isDeleting}
          >
            {isDeleting
              ? t('campaigns.deleting', { defaultValue: 'Deleting...' })
              : t('campaigns.actions.deleteConfirm', { defaultValue: 'Delete action' })}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
