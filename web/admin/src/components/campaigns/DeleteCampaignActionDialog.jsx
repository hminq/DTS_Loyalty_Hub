import { useState } from 'react'

import { deleteCampaignAction } from '../../api/campaignsApi'
import { Button } from '../ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '../ui/dialog'

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
      await deleteCampaignAction(campaignId, action.actionId)
      onOpenChange(false)
      if (onSuccess) {
        onSuccess()
      }
    } catch (error) {
      const code = error.code || ''
      if (code === 'CAMPAIGN_NOT_DRAFT') {
        onOpenChange(false)
        if (onNotDraft) {
          onNotDraft(error.message || t('campaigns.errors.notDraft'))
        }
        return
      }
      if (code === 'CAMPAIGN_LAST_ACTION') {
        onOpenChange(false)
        if (onLastAction) {
          onLastAction(
            error.message ||
              t('campaigns.errors.lastAction', {
                defaultValue: 'A draft campaign must have at least one reward action.',
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
    <Dialog open={open} onOpenChange={(val) => (!isDeleting ? onOpenChange(val) : null)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {t('campaigns.actions.deleteTitle', { defaultValue: 'Delete reward action' })}
          </DialogTitle>
          <DialogDescription>
            {t('campaigns.actions.deleteDescription', {
              actionType: actionTypeLabel,
              order: action.executeOrder ?? 1,
              defaultValue: `Are you sure you want to delete reward action "${actionTypeLabel}" (order #${action.executeOrder ?? 1})? This action cannot be undone.`,
            })}
          </DialogDescription>
        </DialogHeader>

        {errorMessage ? (
          <div className="rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
            {errorMessage}
          </div>
        ) : null}

        <DialogFooter>
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
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
