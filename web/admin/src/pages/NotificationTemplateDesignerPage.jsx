import { CaretLeftIcon, CopyIcon, CheckIcon, PencilSimpleIcon, TrashIcon, PlusIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'

import { createNotificationTemplate, getNotificationCodes, getNotificationTemplate, updateNotificationTemplate } from '../api/notificationsApi'
import { NotificationVariableDialog } from '../components/notifications/NotificationVariableDialog'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { Card, CardContent } from '../components/ui/card'
import { Input } from '../components/ui/input'
import { HighlightedInput } from '../components/ui/highlighted-input'
import { HighlightedTextarea } from '../components/ui/highlighted-textarea'

export function NotificationTemplateDesignerPage() {
  const { id } = useParams()
  const isEditing = id !== 'new' && Boolean(id)
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [codes, setCodes] = useState([])
  const [formData, setFormData] = useState({
    name: '', notificationCode: '', channel: 'PUSH', language: 'vi',
    titleTemplate: '', bodyTemplate: '', variables: [], isActive: false,
  })
  const [isLoading, setIsLoading] = useState(isEditing)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const [variableDialog, setVariableDialog] = useState({ open: false, index: null, variable: null })
  const [copiedVariable, setCopiedVariable] = useState(null)
  const [lastFocus, setLastFocus] = useState({ field: 'bodyTemplate', start: 0, end: 0 })

  useEffect(() => {
    let current = true
    async function load() {
      try {
        const notificationCodes = await getNotificationCodes()
        if (!current) return
        setCodes(notificationCodes || [])
        if (isEditing) {
          const template = await getNotificationTemplate(id)
          if (!current) return
          let titleTemplate = template.titleTemplate || ''
          let bodyTemplate = template.bodyTemplate || ''
          const variables = []
          for (const variable of template.variables || []) {
            if (variable.sourceType === 'FIXED') {
              const placeholder = `{{${variable.name}}}`
              const fixedValue = variable.fixedValue || ''
              titleTemplate = titleTemplate.split(placeholder).join(fixedValue)
              bodyTemplate = bodyTemplate.split(placeholder).join(fixedValue)
              continue
            }
            variables.push({
              ...variable,
              sourceType: variable.sourceType === 'REQUEST' ? 'INPUT' : variable.sourceType,
            })
          }
          setFormData({
            name: template.name || '', notificationCode: template.notificationCode || '',
            channel: template.channel || 'PUSH', language: template.language || 'vi',
            titleTemplate, bodyTemplate, variables,
            isActive: template.isActive ?? false,
          })
        }
      } catch (loadError) {
        if (current) setError(loadError.message || 'Failed to load template')
      } finally {
        if (current) setIsLoading(false)
      }
    }
    load()
    return () => { current = false }
  }, [id, isEditing])

  const selectedCode = codes.find((code) => code.code === formData.notificationCode)
  const inputFields = selectedCode?.inputFields || selectedCode?.requestFields || []
  const systemFields = selectedCode?.systemFields || []

  function handleChange(event) {
    const { name, value } = event.target
    setFormData((current) => ({ ...current, [name]: value }))
  }

  function handleSelect(event) {
    setLastFocus({ field: event.target.name, start: event.target.selectionStart, end: event.target.selectionEnd })
  }

  function insertVariable(name) {
    const field = ['titleTemplate', 'bodyTemplate'].includes(lastFocus.field) ? lastFocus.field : 'bodyTemplate'
    const current = formData[field] || ''
    const start = ['titleTemplate', 'bodyTemplate'].includes(lastFocus.field) ? lastFocus.start : current.length
    const end = ['titleTemplate', 'bodyTemplate'].includes(lastFocus.field) ? lastFocus.end : current.length
    const value = `{{${name}}}`
    setFormData((data) => ({ ...data, [field]: current.slice(0, start) + value + current.slice(end) }))
    setLastFocus({ field, start: start + value.length, end: start + value.length })
    navigator.clipboard?.writeText(value).catch(() => {})
    setCopiedVariable(name)
    window.setTimeout(() => setCopiedVariable(null), 1500)
  }

  function openVariableDialog(index = null) {
    setVariableDialog({ open: true, index, variable: index === null ? null : formData.variables[index] })
  }

  function closeVariableDialog() {
    setVariableDialog({ open: false, index: null, variable: null })
  }

  function saveVariable(variable) {
    setFormData((current) => {
      const variables = [...current.variables]
      if (variableDialog.index === null) variables.push(variable)
      else {
        const previousName = variables[variableDialog.index].name
        variables[variableDialog.index] = variable
        if (previousName !== variable.name) {
          const previousPlaceholder = `{{${previousName}}}`
          const nextPlaceholder = `{{${variable.name}}}`
          return {
            ...current,
            variables,
            titleTemplate: current.titleTemplate.split(previousPlaceholder).join(nextPlaceholder),
            bodyTemplate: current.bodyTemplate.split(previousPlaceholder).join(nextPlaceholder),
          }
        }
      }
      return { ...current, variables }
    })
    closeVariableDialog()
  }

  function deleteVariable(index) {
    const variable = formData.variables[index]
    const placeholder = `{{${variable.name}}}`
    if (formData.titleTemplate.includes(placeholder) || formData.bodyTemplate.includes(placeholder)) {
      setError(t('notifications.variables.deleteInUse', 'Remove this variable from the title/body before deleting it.'))
      return
    }
    setFormData((current) => ({ ...current, variables: current.variables.filter((_, itemIndex) => itemIndex !== index) }))
  }

  async function handleSave() {
    setError('')
    const placeholders = [...`${formData.titleTemplate} ${formData.bodyTemplate}`.matchAll(/\{\{\s*([A-Za-z][A-Za-z0-9_]*)\s*\}\}/g)]
      .map((match) => match[1])
    const configuredNames = new Set(formData.variables.map((variable) => variable.name.toLowerCase()))
    const undefinedNames = [...new Set(placeholders.filter((name) => !configuredNames.has(name.toLowerCase())))]
    if (undefinedNames.length > 0) {
      setError(`Các variable chưa được cấu hình: ${undefinedNames.join(', ')}.`)
      return
    }

    setIsSaving(true)
    try {
      if (isEditing) await updateNotificationTemplate(id, formData)
      else await createNotificationTemplate(formData)
      navigate('/notification-templates')
    } catch (saveError) {
      setError(saveError.message || 'Failed to save template')
      setIsSaving(false)
    }
  }

  if (isLoading) return <div className="p-8 text-center">{t('common.loading', 'Loading...')}</div>

  return (
    <>
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/notification-templates')}><CaretLeftIcon size={18} /></Button>
        <PageHeader
          eyebrow={t('notifications.eyebrow', 'Cấu hình')}
          title={isEditing ? t('notifications.editTitle', 'Edit template') : t('notifications.createTitle', 'Create template')}
          description={t('notifications.designerDescription', 'Design a notification template and configure its variables.')}
        />
        <div className="ml-auto"><Button onClick={handleSave} disabled={isSaving}>{isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}</Button></div>
      </div>

      {error ? <p className="mb-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">{error}</p> : null}

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Card><CardContent className="space-y-4 pt-6">
            <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.name', 'Template name')}<Input name="name" value={formData.name} onChange={handleChange} required /></label>
            <div className="grid gap-4 md:grid-cols-2">
              <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.notificationCode', 'Notification code')}
                <select name="notificationCode" value={formData.notificationCode} onChange={handleChange} required className="h-10 rounded-md border border-input bg-background px-3 text-sm">
                  <option value="">{t('notifications.fields.selectCode', 'Select notification code')}</option>
                  {codes.map((code) => <option key={code.code} value={code.code}>{code.displayName} ({code.code})</option>)}
                </select>
              </label>
              <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.channel', 'Channel')}
                <select name="channel" value={formData.channel} onChange={handleChange} className="h-10 rounded-md border border-input bg-background px-3 text-sm"><option value="PUSH">Push</option><option value="EMAIL">Email</option><option value="SMS">SMS</option><option value="IN_APP">In-App</option></select>
              </label>
              <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.language', 'Language')}
                <select name="language" value={formData.language} onChange={handleChange} className="h-10 rounded-md border border-input bg-background px-3 text-sm"><option value="vi">Vietnamese (vi)</option><option value="en">English (en)</option></select>
              </label>
              <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.status', 'Status')}
                <select value={formData.isActive ? 'true' : 'false'} onChange={(event) => setFormData((data) => ({ ...data, isActive: event.target.value === 'true' }))} className="h-10 rounded-md border border-input bg-background px-3 text-sm"><option value="false">Inactive</option><option value="true">Active</option></select>
              </label>
            </div>
          </CardContent></Card>

          <Card><CardContent className="space-y-4 pt-6">
            <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.titleTemplate', 'Title template')}
              <HighlightedInput name="titleTemplate" value={formData.titleTemplate} onChange={handleChange} onSelect={handleSelect} onClick={handleSelect} onKeyUp={handleSelect} placeholder="Welcome {{CustomerName}}" />
            </label>
            <label className="grid gap-2 text-sm font-medium">{t('notifications.fields.bodyTemplate', 'Body template')}
              <HighlightedTextarea name="bodyTemplate" value={formData.bodyTemplate} onChange={handleChange} onSelect={handleSelect} onClick={handleSelect} onKeyUp={handleSelect} rows={10} placeholder="Enter notification body..." />
            </label>
          </CardContent></Card>
        </div>

        <Card className="h-fit lg:sticky lg:top-6">
          <div className="flex items-center justify-between border-b border-border px-6 py-4">
            <div>
              <h3 className="font-semibold">{t('notifications.variables.title', 'Available Variables')}</h3>
              <p className="mt-1 text-xs text-muted-foreground">{t('notifications.variables.description', 'Variables saved in this template configuration.')}</p>
            </div>
            <Button variant="outline" size="sm" onClick={() => openVariableDialog()}><PlusIcon size={14} /> Add</Button>
          </div>
          <CardContent className="p-0">
            {formData.variables.length === 0 ? (
              <div className="p-6 text-center text-sm text-muted-foreground">No variables configured for this template.</div>
            ) : (
              <ul className="divide-y divide-border">
                {formData.variables.map((variable, index) => (
                  <li key={`${variable.name}-${index}`} className="px-6 py-3">
                    <div className="flex items-center justify-between gap-3">
                      <div className="flex min-w-0 flex-1 items-center gap-3">
                        <button type="button" className="shrink-0 font-mono text-sm font-bold text-primary" onClick={() => insertVariable(variable.name)}>
                          {`{{${variable.name}}}`}
                        </button>
                        {variable.description ? <span className="truncate text-xs text-muted-foreground">{variable.description}</span> : null}
                      </div>
                      <div className="flex shrink-0 gap-1">
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => insertVariable(variable.name)} title="Insert">{copiedVariable === variable.name ? <CheckIcon size={14} /> : <CopyIcon size={14} />}</Button>
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => openVariableDialog(index)} title="Edit"><PencilSimpleIcon size={14} /></Button>
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => deleteVariable(index)} title="Delete"><TrashIcon size={14} /></Button>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
      </div>

      <NotificationVariableDialog
        open={variableDialog.open}
        variable={variableDialog.variable}
        inputFields={inputFields}
        systemFields={systemFields}
        onClose={closeVariableDialog}
        onSave={saveVariable}
        t={t}
      />
    </>
  )
}
