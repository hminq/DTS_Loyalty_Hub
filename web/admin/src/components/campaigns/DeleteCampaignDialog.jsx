import { useState } from 'react'

import { deleteCampaign } from '../../api/campaignsApi'
import { Button } from '../ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '../ui/dialog'

export function DeleteCampaignDialog({
  open,
  onOpenChange,
  campaign,
  onSuccess,
  onNotFound,
  onNotDraft,
  t,
}) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  if (!campaign) return null

  async function handleDelete() {
    setIsDeleting(true)
    setErrorMessage('')

    try {
      await deleteCampaign(campaign.campaignId)
      onOpenChange(false)
      if (onSuccess) {
        onSuccess()
      }
    } catch (error) {
      const code = error.code || ''
      if (code === 'CAMPAIGN_NOT_FOUND') {
        onOpenChange(false)
        if (onNotFound) {
          onNotFound(error.message || t('campaigns.errors.notFound'))
        }
        return
      }
      if (code === 'CAMPAIGN_NOT_DRAFT') {
        if (onNotDraft) {
          onNotDraft(error.message || t('campaigns.errors.notDraft'))
        }
      }
      setErrorMessage(
        error.message || t('campaigns.errors.deleteFailed', { defaultValue: 'Failed to delete campaign.' }),
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
            {t('campaigns.deleteTitle', { defaultValue: 'Delete campaign' })}
          </DialogTitle>
          <DialogDescription>
            {t('campaigns.deleteDescription', {
              name: campaign.campaignName || campaign.campaignId,
              defaultValue: `Are you sure you want to delete campaign "${campaign.campaignName || campaign.campaignId}"? All draft configuration and reward actions will be permanently deleted. This action cannot be undone.`,
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
              : t('campaigns.deleteConfirm', { defaultValue: 'Delete campaign' })}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
