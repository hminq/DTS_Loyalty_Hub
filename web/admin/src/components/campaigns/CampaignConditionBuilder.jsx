import { PlusIcon, TrashIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'
import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'
import { createConditionRow } from './campaignConditions'

export function CampaignConditionBuilder({
  formValues,
  setFormValue,
  selectedVersion,
  errors,
  disabled
}) {
  const { t } = useTranslation()
  
  if (!selectedVersion) {
    return (
      <div className="space-y-4">
        <label className="block text-sm font-medium text-foreground">
          {t('campaigns.fields.condition', { defaultValue: 'Condition' })}
        </label>
        <p className="text-sm text-muted-foreground">
          {t('campaigns.messages.selectVersionFirst', { defaultValue: 'Select an event type first' })}
        </p>
      </div>
    )
  }

  const mode = formValues.conditionMode || 'MATCH_ALL'
  const predicates = formValues.conditionPredicates || []
  const conditionError = errors?.condition
  const rowErrors = errors?.conditionRows || []

  const modeOptions = [
    {
      value: 'MATCH_ALL',
      label: t('campaigns.condition.allValidEvents', { defaultValue: 'All valid events' }),
    },
    {
      value: 'MATCH_FIELDS',
      label: t('campaigns.condition.matchFields', { defaultValue: 'Match fields' }),
    },
  ]
  if (mode === 'UNSUPPORTED') {
    modeOptions.push({
      value: 'UNSUPPORTED',
      label: t('campaigns.condition.unsupported', { defaultValue: 'Unsupported configuration' }),
    })
  }

  const fieldOptions = (selectedVersion.conditionFields || []).map((field) => ({
    value: field.code,
    label: field.label,
  }))

  const booleanOptions = [
    { value: 'true', label: 'true' },
    { value: 'false', label: 'false' },
  ]

  const handleModeChange = (newMode) => {
    setFormValue('conditionMode', newMode)
    if (newMode === 'MATCH_FIELDS' && predicates.length === 0) {
      setFormValue('conditionPredicates', [createConditionRow()])
    }
  }

  const handleAddRow = () => {
    setFormValue('conditionPredicates', [...predicates, createConditionRow()])
  }

  const handleRemoveRow = (idx) => {
    const next = [...predicates]
    next.splice(idx, 1)
    setFormValue('conditionPredicates', next)
  }

  const handleRowChange = (idx, field, val) => {
    const next = [...predicates]
    const row = { ...next[idx], [field]: val }
    if (field === 'field') {
      row.operator = ''
      row.value = ''
    } else if (field === 'operator') {
      row.value = ''
    }
    next[idx] = row
    setFormValue('conditionPredicates', next)
  }

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground">
          {t('campaigns.fields.condition', { defaultValue: 'Condition' })}
        </label>
        <Combobox
          value={mode}
          onValueChange={handleModeChange}
          options={modeOptions}
          placeholder={t('campaigns.fields.condition', { defaultValue: 'Condition' })}
          searchPlaceholder={t('campaigns.fields.condition', { defaultValue: 'Condition' })}
          emptyText={t('campaigns.form.noOptions', { defaultValue: 'No options found.' })}
          ariaLabel={t('campaigns.fields.condition', { defaultValue: 'Condition' })}
          shouldFilter={false}
          disabled={disabled || mode === 'UNSUPPORTED'}
        />
        {conditionError && (
          <p className="text-sm font-medium text-destructive">{conditionError}</p>
        )}
      </div>

      {mode === 'UNSUPPORTED' && (
        <div className="rounded-md border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {t('campaigns.messages.unsupportedConditionWarning', { defaultValue: 'This persisted condition is not supported by the current builder. It remains unchanged. Select another event type to intentionally replace it, or cancel editing.' })}
        </div>
      )}

      {mode === 'MATCH_FIELDS' && (
        <div className="space-y-4 rounded-md border border-border bg-muted/30 p-4">
          {predicates.map((row, idx) => {
            const fieldMeta = (selectedVersion.conditionFields || []).find(f => f.code === row.field)
            const operatorOptions = (fieldMeta?.operators || []).map((operator) => ({
              value: operator,
              label: t(`campaigns.conditionOperators.${operator}`, { defaultValue: operator }),
            }))
            const rowErr = rowErrors[idx] || {}
            const hasRowError = Boolean(rowErr.field || rowErr.incomplete)
            
            return (
              <div key={row.rowId} className="space-y-2">
                <div className="flex flex-wrap items-center gap-2">
                  <div className="flex-1 min-w-[200px]">
                    <Combobox
                      value={row.field}
                      onValueChange={(value) => handleRowChange(idx, 'field', value)}
                      options={fieldOptions}
                      placeholder={t('campaigns.fields.conditionFieldSelect', { defaultValue: '-- Select field --' })}
                      searchPlaceholder={t('campaigns.fields.conditionFieldSelect', { defaultValue: '-- Select field --' })}
                      emptyOptionLabel={t('campaigns.fields.conditionFieldSelect', { defaultValue: '-- Select field --' })}
                      emptyText={t('campaigns.form.noOptions', { defaultValue: 'No options found.' })}
                      ariaLabel={t('campaigns.fields.conditionFieldSelect', { defaultValue: 'Select field' })}
                      disabled={disabled}
                      invalid={hasRowError}
                    />
                  </div>

                  <div className="flex-1 min-w-[150px]">
                    <Combobox
                      value={row.operator}
                      onValueChange={(value) => handleRowChange(idx, 'operator', value)}
                      options={operatorOptions}
                      placeholder={t('campaigns.fields.conditionOperatorSelect', { defaultValue: '-- Select operator --' })}
                      searchPlaceholder={t('campaigns.fields.conditionOperatorSelect', { defaultValue: '-- Select operator --' })}
                      emptyOptionLabel={t('campaigns.fields.conditionOperatorSelect', { defaultValue: '-- Select operator --' })}
                      emptyText={t('campaigns.form.noOptions', { defaultValue: 'No options found.' })}
                      ariaLabel={t('campaigns.fields.conditionOperatorSelect', { defaultValue: 'Select operator' })}
                      disabled={disabled || !fieldMeta}
                      invalid={Boolean(rowErr.operator || rowErr.incomplete)}
                    />
                  </div>

                  <div className="flex-1 min-w-[200px]">
                    {fieldMeta?.dataType === 'BOOLEAN' ? (
                      <Combobox
                        value={row.value}
                        onValueChange={(value) => handleRowChange(idx, 'value', value)}
                        options={booleanOptions}
                        placeholder={t('campaigns.fields.conditionValueSelect', { defaultValue: '-- Select value --' })}
                        searchPlaceholder={t('campaigns.fields.conditionValueSelect', { defaultValue: '-- Select value --' })}
                        emptyOptionLabel={t('campaigns.fields.conditionValueSelect', { defaultValue: '-- Select value --' })}
                        emptyText={t('campaigns.form.noOptions', { defaultValue: 'No options found.' })}
                        ariaLabel={t('campaigns.fields.conditionValueSelect', { defaultValue: 'Select value' })}
                        disabled={disabled || !row.operator}
                        invalid={Boolean(rowErr.value || rowErr.incomplete)}
                        shouldFilter={false}
                      />
                    ) : fieldMeta?.dataType === 'NUMBER' ? (
                      <Input
                        type="number"
                        value={row.value}
                        onChange={(e) => handleRowChange(idx, 'value', e.target.value)}
                        disabled={disabled || !row.operator}
                        placeholder={t('campaigns.fields.conditionValue', { defaultValue: 'Value' })}
                        aria-invalid={Boolean(rowErr.value || rowErr.incomplete)}
                      />
                    ) : (
                      <Input
                        type="text"
                        value={row.value}
                        onChange={(e) => handleRowChange(idx, 'value', e.target.value)}
                        disabled={disabled || !row.operator}
                        placeholder={t('campaigns.fields.conditionValue', { defaultValue: 'Value' })}
                        aria-invalid={Boolean(rowErr.value || rowErr.incomplete)}
                      />
                    )}
                  </div>

                  {!disabled && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      onClick={() => handleRemoveRow(idx)}
                      className="h-8 w-8 shrink-0 text-destructive hover:bg-destructive/10 transition-colors"
                      title={t('campaigns.actions.removeCondition', { defaultValue: 'Remove condition' })}
                    >
                      <TrashIcon size={15} />
                    </Button>
                  )}
                </div>
                
                {/* Row Errors */}
                {(rowErr.field || rowErr.operator || rowErr.value || rowErr.incomplete || rowErr.duplicate) && (
                  <p className="text-[13px] font-medium text-destructive">
                    {[rowErr.field, rowErr.operator, rowErr.value, rowErr.incomplete, rowErr.duplicate].filter(Boolean).join('. ')}
                  </p>
                )}
              </div>
            )
          })}

          {!disabled && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handleAddRow}
              className="mt-2"
            >
              <PlusIcon size={14} data-icon="inline-start" />
              {t('campaigns.actions.addCondition', { defaultValue: 'Add condition' })}
            </Button>
          )}
        </div>
      )}
    </div>
  )
}
