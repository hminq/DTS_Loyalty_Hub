import { Combobox } from '../ui/combobox'
import { Field, FieldError, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
function getDecimalStep(scale) {
  return Number.isInteger(scale) && scale >= 0 ? 10 ** -scale : 'any'
}

function isTargetCompatible(target, actionType) {
  if (!target || !actionType) return false
  return target.targetKind === actionType.requiredTargetKind
}

function getCompatibleActionTypes(actionTypes = [], eventVersion) {
  if (!eventVersion) return []
  return actionTypes.filter((actionType) =>
    (eventVersion.targets || []).some((target) =>
      isTargetCompatible(target, actionType),
    ),
  )
}

export function CampaignActionConfigurationFields({
  prefix = '',
  action,
  options = {},
  eventDefinition = null,
  fieldErrors = {},
  isSubmitting = false,
  onChange,
  t,
}) {
  const eventTargets = eventDefinition?.targets || []
  const hasCampaignContext = Boolean(eventDefinition && eventTargets.length > 0)

  const actionTypeOptions = getCompatibleActionTypes(
    options.actionTypes,
    eventDefinition,
  )

  const selectedActionDef = (options.actionTypes || []).find(
    (actionType) => actionType.value === action.actionType,
  )

  const targetOptions = eventTargets.map((target) => {
    const isCompatible = selectedActionDef
      ? isTargetCompatible(target, selectedActionDef)
      : false

    return {
      ...target,
      disabled: !isCompatible,
    }
  })

  function handleActionTypeChange(nextActionType) {
    const nextDef = actionTypeOptions.find((actionType) => actionType.value === nextActionType)

    let nextTarget = action.targetSelector
    if (nextTarget) {
      const currentTargetDef = eventTargets.find((target) => target.value === nextTarget)
      if (!isTargetCompatible(currentTargetDef, nextDef)) {
        nextTarget = ''
      }
    }

    const nextParams = {}
    if (nextDef) {
      for (const paramDef of nextDef.parameters || []) {
        if (action.parameters?.[paramDef.code] !== undefined) {
          nextParams[paramDef.code] = action.parameters[paramDef.code]
        }
      }
    }

    onChange({
      ...action,
      actionType: nextActionType,
      targetSelector: nextTarget,
      parameters: nextParams,
    })
  }

  function handleTargetChange(nextTarget) {
    onChange({ ...action, targetSelector: nextTarget })
  }

  function handleParameterChange(code, value) {
    onChange({
      ...action,
      parameters: {
        ...(action.parameters || {}),
        [code]: value,
      },
    })
  }

  const actionTypeDisabled = !hasCampaignContext || isSubmitting
  const targetDisabled = actionTypeDisabled || !action.actionType
  const parametersDisabled = targetDisabled || !action.targetSelector

  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <Field
        invalid={Boolean(fieldErrors[`${prefix}actionType`])}
        disabled={actionTypeDisabled}
      >
        <FieldLabel>
          {t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}
        </FieldLabel>
        <Combobox
          value={action.actionType}
          onValueChange={handleActionTypeChange}
          options={actionTypeOptions}
          placeholder={t('campaigns.form.selectActionType', {
            defaultValue: 'Select action type',
          })}
          emptyOptionLabel={t('campaigns.form.selectActionType', {
            defaultValue: 'Select action type',
          })}
          ariaLabel={t('campaigns.form.actionTypeLabel', { defaultValue: 'Action type' })}
          disabled={actionTypeDisabled}
        />
        {fieldErrors[`${prefix}actionType`] ? (
          <FieldError>{fieldErrors[`${prefix}actionType`]}</FieldError>
        ) : null}
      </Field>

      <Field
        invalid={Boolean(fieldErrors[`${prefix}targetSelector`])}
        disabled={targetDisabled}
      >
        <FieldLabel>
          {t('campaigns.form.targetSelectorLabel', { defaultValue: 'Target' })}
        </FieldLabel>
        <Combobox
          value={action.targetSelector}
          onValueChange={handleTargetChange}
          options={targetOptions}
          placeholder={t('campaigns.form.selectTarget', {
            defaultValue: 'Select target',
          })}
          emptyOptionLabel={t('campaigns.form.selectTarget', {
            defaultValue: 'Select target',
          })}
          disabled={targetDisabled}
          ariaLabel={t('campaigns.form.targetSelectorLabel', { defaultValue: 'Target' })}
        />
        {fieldErrors[`${prefix}targetSelector`] ? (
          <FieldError>{fieldErrors[`${prefix}targetSelector`]}</FieldError>
        ) : null}
      </Field>

      {(selectedActionDef?.parameters || []).map((paramDef) => {
        const fieldKey = `${prefix}parameters.${paramDef.code}`
        const isSupported = paramDef.dataType === 'DECIMAL'
        const step = getDecimalStep(paramDef.scale)
        const minimum =
          typeof step === 'number' && paramDef.minimumExclusive != null
            ? Number(paramDef.minimumExclusive) + step
            : undefined
        const unsupportedMessage = t('campaigns.errors.parameterTypeUnsupported', {
          type: paramDef.dataType,
          defaultValue: `Parameter type ${paramDef.dataType} is not supported.`,
        })

        return (
          <Field
            key={paramDef.code}
            invalid={Boolean(fieldErrors[fieldKey]) || !isSupported}
            disabled={parametersDisabled || !isSupported}
          >
            <FieldLabel>
              {paramDef.label || paramDef.code}
              {paramDef.required ? ' *' : ''}
            </FieldLabel>
            <Input
              type="number"
              inputMode="decimal"
              step={step}
              min={minimum}
              max={paramDef.maximum != null ? paramDef.maximum : undefined}
              value={action.parameters?.[paramDef.code] ?? ''}
              onChange={(e) => handleParameterChange(paramDef.code, e.target.value)}
              placeholder=""
              aria-invalid={Boolean(fieldErrors[fieldKey])}
              disabled={parametersDisabled || !isSupported}
            />
            {fieldErrors[fieldKey] || !isSupported ? (
              <FieldError>{fieldErrors[fieldKey] || unsupportedMessage}</FieldError>
            ) : null}
          </Field>
        )
      })}
    </div>
  )
}
