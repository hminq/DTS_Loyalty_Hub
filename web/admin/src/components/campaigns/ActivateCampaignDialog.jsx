import {
  CalendarCheckIcon,
  CircleNotchIcon,
  RocketLaunchIcon,
  WarningCircleIcon,
} from '@phosphor-icons/react'
import { useState } from 'react'

import { activateCampaign } from '../../api/campaignsApi'
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '../ui/alert-dialog'
import { Button } from '../ui/button'
import { formatCampaignSchedule } from './campaignFormatters'
import { describeCampaignCondition, describeCampaignAction } from './campaignPresentation'

export function ActivateCampaignDialog({
  open,
  onOpenChange,
  campaign,
  onSuccess,
  options = {},
  t,
}) {
  const [isActivating, setIsActivating] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  if (!campaign) return null

  async function handleActivate() {
    setIsActivating(true)
    setErrorMessage('')

    try {
      const updatedCampaign = await activateCampaign(campaign.campaignId)
      onOpenChange(false)
      if (onSuccess) {
        onSuccess(updatedCampaign)
      }
    } catch (error) {
      const code = error.code || ''
      if (code === 'CAMPAIGN_ACTIONS_REQUIRED') {
        setErrorMessage(
          error.message ||
            t('campaigns.activate.errors.actionsRequired', {
              defaultValue:
                'Campaign must have at least one reward action before it can be activated.',
            }),
        )
        return
      }
      if (code === 'CAMPAIGN_ALREADY_ACTIVE') {
        setErrorMessage(
          error.message ||
            t('campaigns.activate.errors.alreadyActive', {
              defaultValue: 'This campaign is already active.',
            }),
        )
        return
      }
      if (code === 'CAMPAIGN_SCHEDULE_EMPTY' || code === 'CAMPAIGN_DATE_RANGE_INVALID') {
        setErrorMessage(
          error.message ||
            t('campaigns.activate.errors.emptySchedule', {
              defaultValue:
                'No scheduled sessions could be generated for the configured date range and cron pattern.',
            }),
        )
        return
      }
      setErrorMessage(
        error.message ||
          t('campaigns.activate.errors.failed', {
            defaultValue: 'Failed to activate campaign.',
          }),
      )
    } finally {
      setIsActivating(false)
    }
  }

  const scheduleLabel = formatCampaignSchedule(
    campaign.scheduleCron,
    campaign.durationHour,
    t,
  )

  return (
    <AlertDialog
      open={open}
      onOpenChange={(val) => (!isActivating ? onOpenChange(val) : null)}
    >
      <AlertDialogContent className="max-w-md overflow-hidden p-6 sm:max-w-lg">
        <div className="flex items-start gap-4">
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-success/10 text-success ring-8 ring-success/5">
            <RocketLaunchIcon size={22} weight="bold" />
          </div>
          <div className="flex-1">
            <AlertDialogTitle className="text-lg font-semibold tracking-tight text-foreground">
              {t('campaigns.activate.title', {
                defaultValue: 'Activate campaign',
              })}
            </AlertDialogTitle>
            <AlertDialogDescription className="mt-1 text-sm text-muted-foreground">
              {t('campaigns.activate.description', {
                name: campaign.campaignName || campaign.campaignId,
                defaultValue: `Activate "${campaign.campaignName || campaign.campaignId}" to start processing loyalty events.`,
              })}
            </AlertDialogDescription>
          </div>
        </div>

        <div className="mt-5 space-y-3">
          <div className="rounded-lg border border-border/70 bg-muted/30 p-3.5 text-xs">
            <div className="flex items-center gap-2 font-semibold text-foreground">
              <CalendarCheckIcon size={16} className="text-primary" weight="bold" />
              <span>
                {t('campaigns.activate.scheduleSummary', {
                  defaultValue: 'Schedule summary',
                })}
              </span>
            </div>
            <p className="mt-1.5 font-medium text-muted-foreground">
              {scheduleLabel}
            </p>
          </div>

          <div className="rounded-lg border border-border/70 bg-muted/30 p-3.5 text-xs">
            <div className="flex flex-col gap-1.5">
              <span className="font-semibold text-foreground">
                {t('campaigns.form.conditionLabel', { defaultValue: 'Condition' })}:
              </span>
              <span className="text-muted-foreground">
                {describeCampaignCondition({ condition: campaign.condition, eventDefinition: campaign.eventDefinition, options, t }).label}
              </span>
            </div>

            <div className="mt-3 flex flex-col gap-1.5">
              <span className="font-semibold text-foreground">
                {t('campaigns.form.actionsTitle', { defaultValue: 'Reward actions' })}:
              </span>
              <ul className="list-disc pl-4 text-muted-foreground space-y-1">
                {(campaign.actions || []).map((action, i) => {
                  const desc = describeCampaignAction({ action, eventDefinition: campaign.eventDefinition, options, t })
                  return (
                    <li key={action.actionId || i}>
                      {desc.actionTypeLabel} → {desc.targetLabel}
                    </li>
                  )
                })}
              </ul>
            </div>
          </div>
        </div>

        {errorMessage ? (
          <div className="mt-3 flex items-center gap-2.5 rounded-md border border-destructive/20 bg-destructive/5 px-3.5 py-2.5 text-xs font-medium text-destructive">
            <WarningCircleIcon size={16} weight="bold" className="shrink-0" />
            <span>{errorMessage}</span>
          </div>
        ) : null}

        <AlertDialogFooter className="mt-6 gap-2">
          <Button
            type="button"
            variant="outline"
            disabled={isActivating}
            onClick={() => onOpenChange(false)}
          >
            {t('common.cancel', { defaultValue: 'Cancel' })}
          </Button>
          <Button
            type="button"
            className="gap-2 bg-success text-success-foreground shadow-sm hover:bg-success/90 hover:shadow"
            disabled={isActivating}
            onClick={handleActivate}
          >
            {isActivating ? (
              <>
                <CircleNotchIcon
                  size={16}
                  className="animate-spin"
                  aria-hidden="true"
                />
                <span>
                  {t('campaigns.activate.activating', {
                    defaultValue: 'Activating...',
                  })}
                </span>
              </>
            ) : (
              <>
                <RocketLaunchIcon size={16} weight="bold" />
                <span>
                  {t('campaigns.activate.confirm', {
                    defaultValue: 'Activate now',
                  })}
                </span>
              </>
            )}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
