import { TrashIcon } from '@phosphor-icons/react'
import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { CampaignActionConfigurationFields } from './CampaignActionConfigurationFields'

export function CampaignActionCard({
  index,
  action,
  options = {},
  eventDefinition = null,
  fieldErrors = {},
  cardError = '',
  isSubmitting = false,
  canRemove = false,
  onChange,
  onRemove,
  t,
}) {
  const prefix = `actions[${index}]`

  function updateField(field, value) {
    onChange(index, { ...action, [field]: value })
  }

  function handleConfigChange(nextAction) {
    onChange(index, nextAction)
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm font-semibold">
          {t('campaigns.form.actionLabel', {
            index: index + 1,
            defaultValue: `Action ${index + 1}`,
          })}
        </CardTitle>
        {canRemove ? (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="h-7 gap-1 px-2 text-xs text-destructive hover:bg-destructive/10 transition-colors"
            onClick={() => onRemove(index)}
            disabled={isSubmitting}
          >
            <TrashIcon size={14} />
            {t('campaigns.form.removeAction', { defaultValue: 'Remove' })}
          </Button>
        ) : null}
      </CardHeader>
      <CardContent>
        <FieldGroup>
          {cardError ? (
            <div className="rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-xs font-medium text-destructive">
              {cardError}
            </div>
          ) : null}

          <CampaignActionConfigurationFields
            prefix={`${prefix}.`}
            action={action}
            options={options}
            eventDefinition={eventDefinition}
            fieldErrors={fieldErrors}
            isSubmitting={isSubmitting}
            onChange={handleConfigChange}
            t={t}
          />

          <div className="grid gap-4 sm:grid-cols-2">
            <Field disabled>
              <FieldLabel>
                {t('campaigns.form.executeOrderLabel', { defaultValue: 'Execution order' })}
              </FieldLabel>
              <Input type="number" value={index + 1} disabled readOnly />
            </Field>
          </div>

          <div className="border-t border-border pt-4">
            <h4 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              {t('campaigns.form.actionLimitsTitle', {
                defaultValue: 'Action execution limits',
              })}
            </h4>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field
                invalid={Boolean(fieldErrors[`${prefix}.totalCount`])}
                disabled={isSubmitting}
              >
                <FieldLabel>
                  {t('campaigns.form.actionLimitTotalLabel', {
                    defaultValue: 'Total action execution limit',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0"
                  step="1"
                  inputMode="numeric"
                  value={action.totalCount ?? ''}
                  onChange={(e) => updateField('totalCount', e.target.value)}
                  placeholder={t('campaigns.detail.unlimited', {
                    defaultValue: 'Unlimited',
                  })}
                  aria-invalid={Boolean(fieldErrors[`${prefix}.totalCount`])}
                  disabled={isSubmitting}
                />
                {fieldErrors[`${prefix}.totalCount`] ? (
                  <FieldError>{fieldErrors[`${prefix}.totalCount`]}</FieldError>
                ) : (
                  <p className="text-[11px] text-muted-foreground">
                    {t('campaigns.form.actionLimitTotalHelper', {
                      defaultValue:
                        'Maximum successful executions of this action across the campaign. Leave empty for unlimited.',
                    })}
                  </p>
                )}
              </Field>

              <Field
                invalid={Boolean(fieldErrors[`${prefix}.sessionCount`])}
                disabled={isSubmitting}
              >
                <FieldLabel>
                  {t('campaigns.form.actionLimitSessionLabel', {
                    defaultValue: 'Action execution limit per session',
                  })}
                </FieldLabel>
                <Input
                  type="number"
                  min="0"
                  step="1"
                  inputMode="numeric"
                  value={action.sessionCount ?? ''}
                  onChange={(e) => updateField('sessionCount', e.target.value)}
                  placeholder={t('campaigns.detail.unlimited', {
                    defaultValue: 'Unlimited',
                  })}
                  aria-invalid={Boolean(fieldErrors[`${prefix}.sessionCount`])}
                  disabled={isSubmitting}
                />
                {fieldErrors[`${prefix}.sessionCount`] ? (
                  <FieldError>{fieldErrors[`${prefix}.sessionCount`]}</FieldError>
                ) : (
                  <p className="text-[11px] text-muted-foreground">
                    {t('campaigns.form.actionLimitSessionHelper', {
                      defaultValue:
                        'Maximum successful executions of this action in each session. Leave empty for unlimited.',
                    })}
                  </p>
                )}
              </Field>
            </div>
          </div>
        </FieldGroup>
      </CardContent>
    </Card>
  )
}
