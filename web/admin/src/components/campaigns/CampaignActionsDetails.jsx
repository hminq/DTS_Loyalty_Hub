import { GiftIcon } from '@phosphor-icons/react'

import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatCampaignNumber } from './campaignFormatters'

export function CampaignActionsDetails({ actions = [], language, t }) {
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
            const config = action.actionConfig || {}
            const actionTypeLabel = t(`campaigns.actionTypes.${action.actionType}`, {
              defaultValue: action.actionType || '—',
            })
            const calculationTypeLabel = t(
              `campaigns.calculationTypes.${config.calculationType}`,
              {
                defaultValue: config.calculationType || '—',
              },
            )
            const recipientLabel = t(`campaigns.recipients.${config.recipient}`, {
              defaultValue: config.recipient || '—',
            })

            return (
              <div
                key={action.actionId || index}
                className="rounded-lg border bg-card p-4 shadow-sm transition-colors"
              >
                <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
                  <div className="flex items-center gap-2">
                    <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                      {action.executeOrder ?? index + 1}
                    </span>
                    <h4 className="text-sm font-semibold text-foreground">
                      {actionTypeLabel}
                    </h4>
                  </div>
                  <div className="text-xs font-medium text-muted-foreground">
                    {t('campaigns.detail.actionId', { defaultValue: 'ID' })}: {action.actionId || '—'}
                  </div>
                </div>

                <div className="mt-3 grid gap-4 sm:grid-cols-4">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                      {t('campaigns.form.calculationTypeLabel', {
                        defaultValue: 'Calculation type',
                      })}
                    </p>
                    <p className="mt-1 text-sm font-medium text-foreground">
                      {calculationTypeLabel}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                      {t('campaigns.form.recipientLabel', { defaultValue: 'Recipient' })}
                    </p>
                    <p className="mt-1 text-sm font-medium text-foreground">
                      {recipientLabel}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                      {t('campaigns.form.amountLabel', { defaultValue: 'Reward amount' })}
                    </p>
                    <p className="mt-1 text-sm font-semibold text-primary">
                      {config.amount != null
                        ? formatCampaignNumber(config.amount, language)
                        : '0'}{' '}
                      {t('campaigns.detail.points', { defaultValue: 'points' })}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                      {t('campaigns.detail.limitsTitle', { defaultValue: 'Limits' })}
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {t('campaigns.detail.totalLimit', { defaultValue: 'Total' })}:{' '}
                      {action.totalCount != null
                        ? formatCampaignNumber(action.totalCount, language)
                        : t('common.unlimited', { defaultValue: 'Unlimited' })}
                      {' / '}
                      {t('campaigns.detail.sessionLimit', { defaultValue: 'Session' })}:{' '}
                      {action.sessionCount != null
                        ? formatCampaignNumber(action.sessionCount, language)
                        : t('common.unlimited', { defaultValue: 'Unlimited' })}
                    </p>
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
