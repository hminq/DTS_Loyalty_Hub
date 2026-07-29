import { ArrowDownIcon, ArrowUpIcon, PlusIcon, TrashIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'
import { Checkbox } from '../ui/checkbox'
import { Combobox } from '../ui/combobox'
import { Field, FieldError, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'

export function EventDefinitionFieldRows({
  fields = [],
  fieldTypeOptions = [],
  onChangeField,
  onAddField,
  onRemoveField,
  onReorderField,
  errors = [],
  disabled = false,
}) {
  const { t } = useTranslation()

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-semibold text-foreground">
          {t('eventDefinitions.schema.fieldsTitle')} ({fields.length})
        </h4>
        {!disabled && (
          <Button type="button" variant="outline" size="sm" onClick={onAddField}>
            <PlusIcon data-icon="inline-start" />
            {t('eventDefinitions.schema.addField')}
          </Button>
        )}
      </div>

      {fields.length === 0 ? (
        <div className="rounded-md border border-dashed border-border p-4 text-center text-xs text-muted-foreground">
          {t('eventDefinitions.schema.noFields')}
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {fields.map((field, index) => {
            const errs = errors[index] || {}
            const selectedType = fieldTypeOptions.find((option) => option.value === field.type)
            const formatOptions = [
              { value: '', label: t('eventDefinitions.schema.formatNone') },
              ...(selectedType?.formats || []).map((format) => ({ value: format, label: format })),
            ]

            return (
              <div
                key={field.id || index}
                className="grid gap-3 rounded-lg border border-border bg-card p-3 shadow-2xs xl:grid-cols-[minmax(180px,1fr)_150px_150px_auto_auto]"
              >
                <Field invalid={Boolean(errs.code)}>
                  <FieldLabel>{t('eventDefinitions.schema.fieldCode')}</FieldLabel>
                  <Input
                    value={field.code}
                    onChange={(e) => onChangeField(index, 'code', e.target.value)}
                    placeholder="e.g. customerId"
                    disabled={disabled}
                    invalid={Boolean(errs.code)}
                    className="font-mono text-xs"
                  />
                  {errs.code && <FieldError>{t(errs.code)}</FieldError>}
                </Field>

                <Field invalid={Boolean(errs.type)}>
                  <FieldLabel>{t('eventDefinitions.schema.fieldType')}</FieldLabel>
                  <Combobox
                    value={field.type}
                    selectedLabel={selectedType?.label}
                    options={fieldTypeOptions}
                    onValueChange={(value) => onChangeField(index, 'type', value)}
                    placeholder={t('eventDefinitions.schema.selectFieldType')}
                    searchPlaceholder={t('eventDefinitions.schema.searchFieldType')}
                    emptyText={t('eventDefinitions.schema.noFieldTypes')}
                    disabled={disabled}
                    invalid={Boolean(errs.type)}
                    shouldFilter={false}
                  />
                  {errs.type && <FieldError>{t(errs.type)}</FieldError>}
                </Field>

                <Field invalid={Boolean(errs.format)} disabled={formatOptions.length <= 1}>
                  <FieldLabel>{t('eventDefinitions.schema.fieldFormat')}</FieldLabel>
                  <Combobox
                    value={field.format || ''}
                    selectedLabel={field.format || t('eventDefinitions.schema.formatNone')}
                    options={formatOptions}
                    onValueChange={(value) => onChangeField(index, 'format', value)}
                    placeholder={t('eventDefinitions.schema.formatNone')}
                    searchPlaceholder={t('eventDefinitions.schema.searchFormat')}
                    disabled={disabled || formatOptions.length <= 1}
                    invalid={Boolean(errs.format)}
                    shouldFilter={false}
                  />
                  {errs.format && <FieldError>{t(errs.format)}</FieldError>}
                </Field>

                <div className="flex min-h-9 flex-wrap items-center gap-4 xl:pt-5">
                  <label className="inline-flex cursor-pointer items-center gap-2 text-xs text-foreground">
                    <Checkbox
                      checked={field.required}
                      onChange={(e) => onChangeField(index, 'required', e.target.checked)}
                      disabled={disabled}
                    />
                    {t('eventDefinitions.schema.fieldRequired')}
                  </label>

                  <label className="inline-flex cursor-pointer items-center gap-2 text-xs text-foreground">
                    <Checkbox
                      checked={field.conditionable}
                      onChange={(e) => onChangeField(index, 'conditionable', e.target.checked)}
                      disabled={disabled}
                    />
                    {t('eventDefinitions.schema.fieldConditionable')}
                  </label>
                </div>

                {!disabled && (
                  <div className="flex items-center justify-end gap-1 xl:pt-5">
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="size-8"
                      disabled={index === 0}
                      onClick={() => onReorderField(index, index - 1)}
                      aria-label={t('eventDefinitions.schema.moveFieldUp')}
                      title={t('eventDefinitions.schema.moveFieldUp')}
                    >
                      <ArrowUpIcon />
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="size-8"
                      disabled={index === fields.length - 1}
                      onClick={() => onReorderField(index, index + 1)}
                      aria-label={t('eventDefinitions.schema.moveFieldDown')}
                      title={t('eventDefinitions.schema.moveFieldDown')}
                    >
                      <ArrowDownIcon />
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="size-8 text-destructive hover:bg-destructive/10"
                      onClick={() => onRemoveField(index)}
                      aria-label={t('eventDefinitions.schema.removeField')}
                      title={t('eventDefinitions.schema.removeField')}
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
