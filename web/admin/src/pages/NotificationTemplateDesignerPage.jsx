import {
  CheckIcon,
  CircleNotchIcon,
  CopyIcon,
  PencilSimpleIcon,
  PlusIcon,
  TrashIcon,
} from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'

import {
  createNotificationTemplate,
  getNotificationCodes,
  getNotificationTemplate,
  updateNotificationTemplate,
} from '../api/notificationsApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { NotificationVariableDialog } from '../components/notifications/NotificationVariableDialog'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { Field, FieldLabel } from '../components/ui/field'
import { HighlightedInput } from '../components/ui/highlighted-input'
import { HighlightedTextarea } from '../components/ui/highlighted-textarea'
import { Input } from '../components/ui/input'
import { Select } from '../components/ui/select'

export function NotificationTemplateDesignerPage() {
  const { id } = useParams()
  const isEditing = id !== 'new' && Boolean(id)

  const { t } = useTranslation()
  const navigate = useNavigate()
  const [codes, setCodes] = useState([])
  const [formData, setFormData] = useState({
    name: '',
    notificationCode: '',
    channel: 'PUSH',
    language: 'vi',
    titleTemplate: '',
    bodyTemplate: '',
    variables: [],
    isActive: false,
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
            name: template.name || '',
            notificationCode: template.notificationCode || '',
            channel: template.channel || 'PUSH',
            language: template.language || 'vi',
            titleTemplate,
            bodyTemplate,
            variables,
            isActive: template.isActive ?? false,
          })
        }
      } catch (loadError) {
        if (current) setError(loadError.message || t('notifications.errors.load', 'Failed to load template'))
      } finally {
        if (current) setIsLoading(false)
      }
    }
    load()
    return () => {
      current = false
    }
  }, [id, isEditing, t])

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

  async function handleSave(event) {
    event.preventDefault()
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
      navigate(isEditing ? `/notification-templates/${id}` : '/notification-templates')
    } catch (saveError) {
      setError(saveError.message || t('notifications.errors.save', 'Failed to save template'))
      setIsSaving(false)
    }
  }

  if (isLoading) {
    return (
      <div className="mt-5 flex h-48 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
        <span className="inline-flex items-center gap-2">
          <CircleNotchIcon className="animate-spin" size={18} aria-hidden="true" />
          {t('notifications.loading', 'Loading template...')}
        </span>
      </div>
    )
  }

  const breadcrumbItems = [
    { label: t('notifications.title', 'Notification Templates'), to: '/notification-templates' },
    ...(isEditing ? [{ label: formData.name || id, to: `/notification-templates/${id}` }] : []),
    { label: isEditing ? t('common.edit', 'Edit') : t('notifications.createTitle', 'Create template') },
  ]

  return (
    <form onSubmit={handleSave} noValidate>
      <PageHeader
        breadcrumb={<Breadcrumb items={breadcrumbItems} />}
        title={isEditing ? t('notifications.editTitle', 'Edit template') : t('notifications.createTitle', 'Create template')}
        description={t('notifications.designerDescription', 'Design a notification template and configure its variables.')}
      />

      {error ? (
        <div className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {error}
        </div>
      ) : null}

      <div className="mt-5 grid gap-6 lg:grid-cols-3">
        {/* Form Fields (2 Cols) */}
        <div className="space-y-6 lg:col-span-2">
          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm font-semibold">
                {t('notifications.detail.generalTitle', 'General information')}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 p-4">
              <Field>
                <FieldLabel>{t('notifications.fields.name', 'Template name')}</FieldLabel>
                <Input name="name" value={formData.name} onChange={handleChange} required />
              </Field>

              <div className="grid gap-4 md:grid-cols-2">
                <Field>
                  <FieldLabel>{t('notifications.fields.notificationCode', 'Notification code')}</FieldLabel>
                  <Select name="notificationCode" value={formData.notificationCode} onChange={handleChange} required>
                    <option value="">{t('notifications.fields.selectCode', 'Select notification code')}</option>
                    {codes.map((code) => (
                      <option key={code.code} value={code.code}>
                        {code.displayName} ({code.code})
                      </option>
                    ))}
                  </Select>
                </Field>

                <Field>
                  <FieldLabel>{t('notifications.fields.channel', 'Channel')}</FieldLabel>
                  <Select name="channel" value={formData.channel} onChange={handleChange}>
                    <option value="PUSH">Push</option>
                    <option value="EMAIL">Email</option>
                    <option value="SMS">SMS</option>
                    <option value="IN_APP">In-App</option>
                  </Select>
                </Field>

                <Field>
                  <FieldLabel>{t('notifications.fields.language', 'Language')}</FieldLabel>
                  <Select name="language" value={formData.language} onChange={handleChange}>
                    <option value="vi">Vietnamese (vi)</option>
                    <option value="en">English (en)</option>
                  </Select>
                </Field>

                <Field>
                  <FieldLabel>{t('notifications.fields.status', 'Status')}</FieldLabel>
                  <Select
                    value={formData.isActive ? 'true' : 'false'}
                    onChange={(event) =>
                      setFormData((data) => ({ ...data, isActive: event.target.value === 'true' }))
                    }
                  >
                    <option value="false">{t('notifications.status.inactive', 'Inactive')}</option>
                    <option value="true">{t('notifications.status.active', 'Active')}</option>
                  </Select>
                </Field>
              </div>
            </CardContent>
          </Card>

          <Card className="rounded-xl border-border/80 shadow-none">
            <CardHeader className="p-4 pb-3">
              <CardTitle className="text-sm font-semibold">
                {t('notifications.detail.contentTitle', 'Template content')}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 p-4">
              <Field>
                <FieldLabel>{t('notifications.fields.titleTemplate', 'Title template')}</FieldLabel>
                <HighlightedInput
                  name="titleTemplate"
                  value={formData.titleTemplate}
                  onChange={handleChange}
                  onSelect={handleSelect}
                  onClick={handleSelect}
                  onKeyUp={handleSelect}
                  placeholder="Welcome {{CustomerName}}"
                />
              </Field>

              <Field>
                <FieldLabel>{t('notifications.fields.bodyTemplate', 'Body template')}</FieldLabel>
                <HighlightedTextarea
                  name="bodyTemplate"
                  value={formData.bodyTemplate}
                  onChange={handleChange}
                  onSelect={handleSelect}
                  onClick={handleSelect}
                  onKeyUp={handleSelect}
                  rows={10}
                  placeholder="Enter notification body..."
                />
              </Field>
            </CardContent>
          </Card>
        </div>

        {/* Variables Panel (1 Col) */}
        <div className="space-y-6 lg:col-span-1">
          <Card className="h-fit rounded-xl border-border/80 shadow-none lg:sticky lg:top-6">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 p-4 pb-3">
              <div>
                <CardTitle className="text-sm font-semibold">
                  {t('notifications.variables.title', 'Available Variables')}
                </CardTitle>
                <CardDescription className="mt-0.5 text-xs text-muted-foreground">
                  {t('notifications.variables.description', 'Variables saved in this template configuration.')}
                </CardDescription>
              </div>
              <Button type="button" variant="outline" size="sm" onClick={() => openVariableDialog()}>
                <PlusIcon size={14} /> {t('common.add', 'Add')}
              </Button>
            </CardHeader>
            <CardContent className="p-0">
              {formData.variables.length === 0 ? (
                <div className="p-6 text-center text-xs text-muted-foreground">
                  {t('notifications.variables.none', 'No variables configured for this template.')}
                </div>
              ) : (
                <ul className="divide-y divide-border/60">
                  {formData.variables.map((variable, index) => (
                    <li key={`${variable.name}-${index}`} className="px-4 py-3 text-xs">
                      <div className="flex items-center justify-between gap-3">
                        <div className="flex min-w-0 flex-1 items-center gap-3">
                          <button
                            type="button"
                            className="shrink-0 font-mono text-sm font-bold text-primary hover:underline"
                            onClick={() => insertVariable(variable.name)}
                          >
                            {`{{${variable.name}}}`}
                          </button>
                          {variable.description ? (
                            <span className="truncate text-xs text-muted-foreground">{variable.description}</span>
                          ) : null}
                        </div>
                        <div className="flex shrink-0 gap-1">
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7"
                            onClick={() => insertVariable(variable.name)}
                            title="Insert"
                          >
                            {copiedVariable === variable.name ? <CheckIcon size={14} /> : <CopyIcon size={14} />}
                          </Button>
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7"
                            onClick={() => openVariableDialog(index)}
                            title="Edit"
                          >
                            <PencilSimpleIcon size={14} />
                          </Button>
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7 text-destructive hover:bg-destructive/10 transition-colors"
                            onClick={() => deleteVariable(index)}
                            title="Delete"
                          >
                            <TrashIcon size={14} />
                          </Button>
                        </div>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Bottom Form Actions Bar */}
      <div className="mt-6 flex items-center justify-end gap-3 border-t border-border pt-6">
        <Button
          type="button"
          variant="outline"
          onClick={() => navigate(isEditing ? `/notification-templates/${id}` : '/notification-templates')}
          disabled={isSaving}
        >
          {t('common.cancel', 'Cancel')}
        </Button>
        <Button type="submit" disabled={isSaving}>
          {isSaving ? (
            <span className="inline-flex items-center gap-1.5">
              <CircleNotchIcon className="animate-spin" size={14} />
              {t('common.saving', 'Saving...')}
            </span>
          ) : (
            t('common.save', 'Save')
          )}
        </Button>
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
    </form>
  )
}
