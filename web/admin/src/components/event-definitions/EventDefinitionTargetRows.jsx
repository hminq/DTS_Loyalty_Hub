import { ArrowDownIcon, ArrowUpIcon, PlusIcon, TrashIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { Field, FieldError, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { getCompatibleIdFields } from './eventDefinitionValidation'

export function EventDefinitionTargetRows({
  targets = [],
  fields = [],
  targetKindOptions = [],
  onChangeTarget,
  onAddTarget,
  onRemoveTarget,
  onReorderTarget,
  errors = [],
  disabled = false,
}) {
  const { t } = useTranslation()
  const compatibleFields = getCompatibleIdFields(fields)
  const idFieldOptions = compatibleFields.map((field) => ({ value: field.code, label: field.code }))

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-semibold text-foreground">
          {t('eventDefinitions.schema.targetsTitle')} ({targets.length})
        </h4>
        {!disabled && (
          <Button type="button" variant="outline" size="sm" onClick={onAddTarget}>
            <PlusIcon data-icon="inline-start" />
            {t('eventDefinitions.schema.addTarget')}
          </Button>
        )}
      </div>

      {targets.length === 0 ? (
        <div className="rounded-md border border-dashed border-border p-4 text-center text-xs text-muted-foreground">
          {t('eventDefinitions.schema.noTargets')}
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {targets.map((target, index) => {
            const errs = errors[index] || {}
            const selectedKind = targetKindOptions.find((option) => option.value === target.kind)
            const selectedIdField = idFieldOptions.find((option) => option.value === target.idField)

            return (
              <div
                key={target.id || index}
                className="grid gap-3 rounded-lg border border-border bg-card p-3 shadow-2xs xl:grid-cols-[minmax(220px,1fr)_170px_minmax(220px,1fr)_auto]"
              >
                <Field invalid={Boolean(errs.selector)}>
                  <FieldLabel>{t('eventDefinitions.schema.targetSelector')}</FieldLabel>
                  <Input
                    value={target.selector}
                    onChange={(e) => onChangeTarget(index, 'selector', e.target.value)}
                    placeholder="e.g. TRANSACTION_CUSTOMER"
                    disabled={disabled}
                    invalid={Boolean(errs.selector)}
                    className="font-mono text-xs uppercase"
                  />
                  {errs.selector && <FieldError>{t(errs.selector)}</FieldError>}
                </Field>

                <Field invalid={Boolean(errs.kind)}>
                  <FieldLabel>{t('eventDefinitions.schema.targetKind')}</FieldLabel>
                  <Combobox
                    value={target.kind}
                    selectedLabel={selectedKind?.label}
                    options={targetKindOptions}
                    onValueChange={(value) => onChangeTarget(index, 'kind', value)}
                    placeholder={t('eventDefinitions.schema.selectTargetKind')}
                    searchPlaceholder={t('eventDefinitions.schema.searchTargetKind')}
                    emptyText={t('eventDefinitions.schema.noTargetKinds')}
                    disabled={disabled}
                    invalid={Boolean(errs.kind)}
                    shouldFilter={false}
                  />
                  {errs.kind && <FieldError>{t(errs.kind)}</FieldError>}
                </Field>

                <Field invalid={Boolean(errs.idField)} disabled={compatibleFields.length === 0}>
                  <FieldLabel>{t('eventDefinitions.schema.targetIdField')}</FieldLabel>
                  <Combobox
                    value={target.idField}
                    selectedLabel={selectedIdField?.label}
                    options={idFieldOptions}
                    onValueChange={(value) => onChangeTarget(index, 'idField', value)}
                    placeholder={t('eventDefinitions.schema.selectIdField')}
                    searchPlaceholder={t('eventDefinitions.schema.searchIdField')}
                    emptyText={t('eventDefinitions.schema.noIdFields')}
                    disabled={disabled || compatibleFields.length === 0}
                    invalid={Boolean(errs.idField)}
                    ariaLabel={t('eventDefinitions.schema.targetIdField')}
                  />
                  {errs.idField && <FieldError>{t(errs.idField)}</FieldError>}
                  {compatibleFields.length === 0 && (
                    <p className="text-[11px] text-warning">
                      {t('eventDefinitions.schema.noCompatibleFieldsHint')}
                    </p>
                  )}
                </Field>

                {!disabled && (
                  <div className="flex items-center justify-end gap-1 xl:pt-5">
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="size-8"
                      disabled={index === 0}
                      onClick={() => onReorderTarget(index, index - 1)}
                      aria-label={t('eventDefinitions.schema.moveTargetUp')}
                      title={t('eventDefinitions.schema.moveTargetUp')}
                    >
                      <ArrowUpIcon />
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="size-8"
                      disabled={index === targets.length - 1}
                      onClick={() => onReorderTarget(index, index + 1)}
                      aria-label={t('eventDefinitions.schema.moveTargetDown')}
                      title={t('eventDefinitions.schema.moveTargetDown')}
                    >
                      <ArrowDownIcon />
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="size-8 text-destructive hover:bg-destructive/10"
                      onClick={() => onRemoveTarget(index)}
                      aria-label={t('eventDefinitions.schema.removeTarget')}
                      title={t('eventDefinitions.schema.removeTarget')}
                    >
                      <TrashIcon />
                    </Button>
                  </div>
                )}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
