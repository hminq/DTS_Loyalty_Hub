import { useTranslation } from 'react-i18next'

import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Field, FieldError, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { Textarea } from '../ui/textarea'

export function EventDefinitionMetadataForm({
  values = {},
  onChange,
  errors = {},
  canEditIdentity = true,
  disabled = false,
}) {
  const { t } = useTranslation()

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('eventDefinitions.form.metadataTitle')}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field invalid={Boolean(errors.code)}>
            <FieldLabel required>{t('eventDefinitions.form.code')}</FieldLabel>
            <Input
              value={values.code || ''}
              onChange={(e) => onChange('code', e.target.value)}
              placeholder="e.g. CUSTOMER_TRANSACTION_SUCCEEDED"
              disabled={disabled || !canEditIdentity}
              className="font-mono text-sm uppercase"
            />
            {errors.code && <FieldError>{t(errors.code)}</FieldError>}
            {!canEditIdentity && (
              <p className="mt-1 text-xs text-muted-foreground">
                {t('eventDefinitions.form.identityLockedHint')}
              </p>
            )}
          </Field>

          <Field invalid={Boolean(errors.routingKey)}>
            <FieldLabel required>{t('eventDefinitions.form.routingKey')}</FieldLabel>
            <Input
              value={values.routingKey || ''}
              onChange={(e) => onChange('routingKey', e.target.value)}
              placeholder="e.g. customer.transaction.succeeded"
              disabled={disabled || !canEditIdentity}
              className="font-mono text-sm lowercase"
            />
            {errors.routingKey && <FieldError>{t(errors.routingKey)}</FieldError>}
          </Field>
        </div>

        <Field invalid={Boolean(errors.name)}>
          <FieldLabel required>{t('eventDefinitions.form.name')}</FieldLabel>
          <Input
            value={values.name || ''}
            onChange={(e) => onChange('name', e.target.value)}
            placeholder="e.g. Customer Transaction Succeeded"
            disabled={disabled}
          />
          {errors.name && <FieldError>{t(errors.name)}</FieldError>}
        </Field>

        <Field invalid={Boolean(errors.description)}>
          <FieldLabel>{t('eventDefinitions.form.description')}</FieldLabel>
          <Textarea
            value={values.description || ''}
            onChange={(e) => onChange('description', e.target.value)}
            placeholder={t('eventDefinitions.form.descriptionPlaceholder')}
            rows={3}
            disabled={disabled}
          />
          {errors.description && <FieldError>{t(errors.description)}</FieldError>}
        </Field>
      </CardContent>
    </Card>
  )
}
