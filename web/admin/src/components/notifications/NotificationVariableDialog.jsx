import { useEffect, useRef, useState } from 'react'
import { Button } from '../ui/button'
import { Input } from '../ui/input'

const emptyVariable = {
  sourceType: 'INPUT',
  sourceKey: '',
  defaultValue: '',
  description: '',
}

function toVariableName(value) {
  if (!value) return ''
  const words = value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .split(/[^A-Za-z0-9]+/)
    .filter(Boolean)
  return words.map((word) => word.charAt(0).toUpperCase() + word.slice(1)).join('')
}

function NotificationVariableDialog({ open, variable, inputFields, systemFields, onClose, onSave, t }) {
  const dialogRef = useRef(null)
  const [form, setForm] = useState(variable || emptyVariable)

  useEffect(() => {
    setForm(variable
      ? {
          ...variable,
          sourceType: variable.sourceType === 'REQUEST' ? 'INPUT' : variable.sourceType,
        }
      : { ...emptyVariable })
  }, [variable, open])

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  function change(name, value) {
    setForm((current) => ({ ...current, [name]: value }))
  }

  function submit(event) {
    event.preventDefault()
    const variableName = toVariableName(form.sourceKey)

    onSave({
      name: variableName,
      sourceType: form.sourceType,
      sourceKey: form.sourceKey,
      fixedValue: null,
      defaultValue: form.defaultValue || null,
      description: form.description?.trim() || null,
    })
  }

  return (
    <dialog ref={dialogRef} onCancel={onClose} className="fixed left-1/2 top-1/2 m-0 w-[min(92vw,520px)] -translate-x-1/2 -translate-y-1/2 rounded-xl border border-border bg-background p-0 text-foreground shadow-2xl backdrop:bg-black/40">
      <form onSubmit={submit} className="space-y-5 p-6">
        <div>
          <h2 className="text-lg font-semibold">{t('notifications.variables.dialogTitle', 'Configure variable')}</h2>
          <p className="mt-1 text-sm text-muted-foreground">{t('notifications.variables.dialogDescription', 'Define how this placeholder gets its value.')}</p>
        </div>

        <label className="grid gap-2 text-sm font-medium">
          {t('notifications.variables.variableDescription', 'Description')} <span className="font-normal text-muted-foreground">({t('common.optional', 'optional')})</span>
          <Input
            value={form.description || ''}
            onChange={(e) => change('description', e.target.value)}
            placeholder={t('notifications.variables.variableDescriptionPlaceholder', 'Describe how this variable is used')}
            maxLength={500}
          />
        </label>

        <label className="grid gap-2 text-sm font-medium">
          {t('notifications.variables.sourceType', 'Source')}
          <select value={form.sourceType} onChange={(e) => change('sourceType', e.target.value)} className="h-10 rounded-md border border-input bg-background px-3 text-sm">
            <option value="INPUT">INPUT</option>
            <option value="SYSTEM">SYSTEM</option>
          </select>
        </label>

        <>
          <label className="grid gap-2 text-sm font-medium">
            {form.sourceType === 'SYSTEM'
              ? t('notifications.variables.systemField', 'System field')
              : t('notifications.variables.inputField', 'Input field')}
            {form.sourceType === 'SYSTEM' ? (
              <select
                value={form.sourceKey || ''}
                onChange={(e) => change('sourceKey', e.target.value)}
                className="h-10 rounded-md border border-input bg-background px-3 text-sm"
                required
              >
                <option value="">{t('notifications.variables.selectSystemField', 'Select a system field')}</option>
                {systemFields.map((field) => <option key={field.key} value={field.key}>{field.displayName} ({field.key})</option>)}
              </select>
            ) : (
              <Input
                value={form.sourceKey || ''}
                onChange={(e) => change('sourceKey', e.target.value)}
                list="notification-input-field-suggestions"
                placeholder={t('notifications.variables.selectInputField', 'Select or enter an input field')}
                required
              />
            )}
            <datalist id="notification-input-field-suggestions">
              {inputFields.map((field) => <option key={field.key} value={field.key}>{field.displayName}</option>)}
            </datalist>
            {form.sourceKey ? (
              <span className="text-xs text-muted-foreground">
                Placeholder: <span className="font-mono font-semibold text-primary">{`{{${toVariableName(form.sourceKey)}}}`}</span>
              </span>
            ) : null}
          </label>
          <label className="grid gap-2 text-sm font-medium">
            {t('notifications.variables.defaultValue', 'Default value')} <span className="font-normal text-muted-foreground">({t('common.optional', 'optional')})</span>
            <Input value={form.defaultValue || ''} onChange={(e) => change('defaultValue', e.target.value)} placeholder={t('notifications.variables.defaultPlaceholder', 'Optional')} />
          </label>
        </>

        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>{t('common.cancel', 'Cancel')}</Button>
          <Button type="submit">{t('common.save', 'Save')}</Button>
        </div>
      </form>
    </dialog>
  )
}

export { NotificationVariableDialog }
