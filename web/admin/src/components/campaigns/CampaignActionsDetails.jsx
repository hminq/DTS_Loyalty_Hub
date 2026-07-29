import { GiftIcon } from '@phosphor-icons/react'

import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatCampaignNumber } from './campaignFormatters'
import { describeCampaignAction } from './campaignPresentation'

export function CampaignActionsDetails({ actions = [], eventType, options = {}, language, t }) {
  const orderedActions = [...(actions || [])].sort(
    (a, b) => (a.executeOrder ?? 0) - (b.executeOrder ?? 0),
  )

  if (orderedActions.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>
            {t('campaigns.form.actionsTitle', { defaultValue: 'Reward actions' })}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center gap-2 rounded-lg border border-dashed py-12 text-muted-foreground">
            <GiftIcon size={32} className="opacity-50" />
            <p className="text-sm font-medium">
              {t('campaigns.detail.noActions', {
                defaultValue: 'No reward actions configured for this campaign.',
              })}
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>
          {t('campaigns.form.actionsTitle', { defaultValue: 'Reward actions' })}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4">
          {orderedActions.map((action, index) => {
            const actionDesc = describeCampaignAction({ action, eventType, options, t })

            return (
              <div
                key={action.actionId || index}
                className="rounded-lg border bg-card p-4 shadow-sm transition-colors"
              >
                <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
                  <div className="flex items-center gap-2">
                    <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                      {actionDesc.executeOrder}
                    </span>
                    <h4 className="text-sm font-semibold text-foreground">
                      {actionDesc.actionTypeLabel}
                    </h4>
                  </div>
                  <div className="text-xs font-medium text-muted-foreground">
                    {t('campaigns.detail.actionId', { defaultValue: 'ID' })}: {action.actionId || '—'}
                  </div>
                </div>

                {!actionDesc.isSupported && (
                  <div className="mt-3 rounded-md bg-amber-50 px-3 py-2 text-xs font-medium text-amber-700">
                    {t('campaigns.detail.unsupportedAction', { defaultValue: 'Warning: Unsupported action configuration.' })}
                  </div>
                )}

                <div className="mt-3 grid gap-4 sm:grid-cols-4">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                      {t('campaigns.form.targetSelectorLabel', { defaultValue: 'Target' })}
                    </p>
                    <p className="mt-1 text-sm font-medium text-foreground">
                      {actionDesc.targetLabel}
                    </p>
                  </div>

                  {actionDesc.parameters.map(param => (
                    <div key={param.code}>
                      <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                        {param.label}
                      </p>
                      <p className={`mt-1 text-sm font-semibold ${param.isKnown ? 'text-primary' : 'text-amber-600'}`}>
                        {param.dataType === 'DECIMAL' && typeof param.value === 'number'
                          ? formatCampaignNumber(param.value, language)
                          : String(param.value)}
                      </p>
                    </div>
                  ))}

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                      {t('campaigns.form.actionLimitsTitle', {
                        defaultValue: 'Action execution limits',
                      })}
                    </p>
                    <div className="mt-1 space-y-0.5 text-xs text-muted-foreground">
                      <div>
                        <span className="font-medium">
                          {t('campaigns.detail.actionTotalLimit', {
                            defaultValue: 'Total action execution limit',
                          })}:
                        </span>{' '}
                        {actionDesc.totalCount != null
                          ? formatCampaignNumber(actionDesc.totalCount, language)
                          : t('campaigns.detail.unlimited', { defaultValue: 'Unlimited' })}
                      </div>
                      <div>
                        <span className="font-medium">
                          {t('campaigns.detail.actionSessionLimit', {
                            defaultValue: 'Action execution limit per session',
                          })}:
                        </span>{' '}
                        {actionDesc.sessionCount != null
                          ? formatCampaignNumber(actionDesc.sessionCount, language)
                          : t('campaigns.detail.unlimited', { defaultValue: 'Unlimited' })}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      </CardContent>
    </Card>
  )
}
