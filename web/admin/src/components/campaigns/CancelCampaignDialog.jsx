import {
  CircleNotchIcon,
  ProhibitIcon,
  WarningCircleIcon,
} from '@phosphor-icons/react'
import { useState } from 'react'

import { cancelCampaign } from '../../api/campaignsApi'
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '../ui/alert-dialog'
import { Button } from '../ui/button'

export function CancelCampaignDialog({
  open,
  onOpenChange,
  campaign,
  onSuccess,
  onNotFound,
  onNotActive,
  t,
}) {
  const [isCancelling, setIsCancelling] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  if (!campaign) return null

  function handleOpenChange(nextOpen) {
    if (isCancelling) return

    if (!nextOpen) {
      setErrorMessage('')
    }
    onOpenChange(nextOpen)
  }

  async function handleCancel() {
    setIsCancelling(true)
    setErrorMessage('')

    try {
      const result = await cancelCampaign(campaign.campaignId)
      onOpenChange(false)
      onSuccess?.(result)
    } catch (error) {
      const code = error.code || ''

      if (code === 'CAMPAIGN_NOT_FOUND') {
        onOpenChange(false)
        onNotFound?.(
          error.message ||
            t('campaigns.errors.notFound', {
              defaultValue: 'Campaign not found.',
            }),
        )
        return
      }

      if (code === 'CAMPAIGN_NOT_ACTIVE') {
        onOpenChange(false)
        onNotActive?.(
          error.message ||
            t('campaigns.cancel.errors.notActive', {
              defaultValue: 'Only an active campaign can be cancelled.',
            }),
        )
        return
      }

      setErrorMessage(
        error.message ||
          t('campaigns.cancel.errors.failed', {
            defaultValue: 'Failed to cancel campaign.',
          }),
      )
    } finally {
      setIsCancelling(false)
    }
  }

  return (
    <AlertDialog open={open} onOpenChange={handleOpenChange}>
      <AlertDialogContent className="max-w-md overflow-hidden p-6 sm:max-w-lg">
        <div className="flex items-start gap-4">
          <div className="flex size-11 shrink-0 items-center justify-center rounded-full bg-destructive/10 text-destructive ring-8 ring-destructive/5">
            <ProhibitIcon className="size-5" weight="bold" aria-hidden="true" />
          </div>
          <div className="flex-1">
            <AlertDialogTitle className="text-lg">
              {t('campaigns.cancel.title', {
                defaultValue: 'Cancel campaign',
              })}
            </AlertDialogTitle>
            <AlertDialogDescription className="mt-1">
              {t('campaigns.cancel.description', {
                name: campaign.campaignName || campaign.campaignId,
                defaultValue: `Cancel "${campaign.campaignName || campaign.campaignId}" and stop rewards that have not been committed.`,
              })}
            </AlertDialogDescription>
          </div>
        </div>

        <div className="mt-5 rounded-lg border border-border bg-muted/30 p-3.5">
          <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
            <WarningCircleIcon className="size-4 text-destructive" weight="bold" aria-hidden="true" />
            <span>
              {t('campaigns.cancel.impactTitle', {
                defaultValue: 'What will happen?',
              })}
            </span>
          </div>
          <p className="mt-1.5 text-xs leading-5 text-muted-foreground">
            {t('campaigns.cancel.impactText', {
              defaultValue:
                'Scheduled and running sessions will be cancelled. Uncommitted rewards will stop, while rewards already committed remain unchanged. This action cannot be undone.',
            })}
          </p>
        </div>

        {errorMessage ? (
          <div
            role="alert"
            className="mt-3 flex items-center gap-2.5 rounded-md border border-destructive/20 bg-destructive/5 px-3.5 py-2.5 text-xs font-medium text-destructive"
          >
            <WarningCircleIcon className="size-4 shrink-0" weight="bold" aria-hidden="true" />
            <span>{errorMessage}</span>
          </div>
        ) : null}

        <AlertDialogFooter className="mt-6">
          <Button
            variant="outline"
            disabled={isCancelling}
            onClick={() => handleOpenChange(false)}
          >
            {t('common.cancel', { defaultValue: 'Cancel' })}
          </Button>
          <Button
            variant="destructive"
            disabled={isCancelling}
            onClick={handleCancel}
          >
            {isCancelling ? (
              <>
                <CircleNotchIcon
                  data-icon="inline-start"
                  className="animate-spin"
                  aria-hidden="true"
                />
                {t('campaigns.cancel.cancelling', {
                  defaultValue: 'Cancelling...',
                })}
              </>
            ) : (
              <>
                <ProhibitIcon data-icon="inline-start" weight="bold" aria-hidden="true" />
                {t('campaigns.cancel.confirm', {
                  defaultValue: 'Cancel campaign',
                })}
              </>
            )}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
