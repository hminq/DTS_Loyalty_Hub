import { CaretLeftIcon, CopyIcon, CheckIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'

import {
  getNotificationTemplate,
  createNotificationTemplate,
  updateNotificationTemplate,
  getNotificationEventTypes,
} from '../api/notificationsApi'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { Card, CardContent } from '../components/ui/card'
import { Input } from '../components/ui/input'
import { HighlightedInput } from '../components/ui/highlighted-input'
import { HighlightedTextarea } from '../components/ui/highlighted-textarea'

export function NotificationTemplateDesignerPage() {
  const { id } = useParams()
  const [searchParams] = useSearchParams()
  const eventTypeCode = searchParams.get('eventTypeCode')
  const isEditing = id !== 'new' && Boolean(id)
  const { t } = useTranslation()
  const navigate = useNavigate()

  const handleBack = () => {
    if (eventTypeCode) {
      navigate(`/notification-templates/events/${eventTypeCode}`)
    } else {
      navigate('/notification-templates')
    }
  }

  const [formData, setFormData] = useState({
    name: '',
    notificationEventTypeId: '',
    channel: 'EMAIL',
    language: 'vi',
    titleTemplate: '',
    bodyTemplate: '',
    isActive: true,
  })
  
  const [eventTypes, setEventTypes] = useState([])
  const [isLoading, setIsLoading] = useState(isEditing)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const [copiedVariable, setCopiedVariable] = useState(null)
  const [lastFocus, setLastFocus] = useState({ field: null, start: 0, end: 0 })

  const handleSelect = (e) => {
    setLastFocus({
      field: e.target.name,
      start: e.target.selectionStart,
      end: e.target.selectionEnd
    })
  }

  useEffect(() => {
    let isCurrent = true

    async function loadData() {
      try {
        const types = await getNotificationEventTypes()
        if (!isCurrent) return
        setEventTypes(types || [])

        if (isEditing) {
          const template = await getNotificationTemplate(id)
          if (!isCurrent) return
          setFormData({
            name: template.name || '',
            notificationEventTypeId: template.notificationEventTypeId || '',
            channel: template.channel || 'EMAIL',
            language: template.language || 'vi',
            titleTemplate: template.titleTemplate || '',
            bodyTemplate: template.bodyTemplate || '',
            isActive: template.isActive ?? true,
          })
        } else if (eventTypeCode && types) {
          const matchedType = types.find(t => t.eventTypeCode === eventTypeCode)
          if (matchedType) {
            setFormData(prev => ({
              ...prev,
              notificationEventTypeId: matchedType.notificationEventTypeId
            }))
          }
        }
      } catch (err) {
        if (isCurrent) setError(err.message || t('errors.loadTemplate', 'Failed to load template.'))
      } finally {
        if (isCurrent) setIsLoading(false)
      }
    }

    loadData()
    return () => { isCurrent = false }
  }, [id, isEditing, t])

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData((prev) => ({ ...prev, [name]: value }))
  }

  const handleSave = async () => {
    setError('')
    setIsSaving(true)
    try {
      if (isEditing) {
        await updateNotificationTemplate(id, formData)
      } else {
        await createNotificationTemplate(formData)
      }
      handleBack()
    } catch (err) {
      setError(err.message || t('errors.saveTemplate', 'Failed to save template.'))
      setIsSaving(false)
    }
  }

  const insertVariable = (varName) => {
    const textToInsert = `{{${varName}}}`
    
    let field = lastFocus.field
    let start = lastFocus.start
    let end = lastFocus.end

    // Default to bodyTemplate if no valid field is currently focused
    if (!field || !['titleTemplate', 'bodyTemplate'].includes(field)) {
      field = 'bodyTemplate'
      start = (formData.bodyTemplate || '').length
      end = (formData.bodyTemplate || '').length
    }

    const currentValue = formData[field] || ''
    const newValue = currentValue.substring(0, start) + textToInsert + currentValue.substring(end)
    
    setFormData((prev) => ({ ...prev, [field]: newValue }))
    
    setLastFocus({
      field,
      start: start + textToInsert.length,
      end: start + textToInsert.length
    })

    // Also copy to clipboard for convenience
    navigator.clipboard.writeText(textToInsert).catch(() => {})
    setCopiedVariable(varName)
    setTimeout(() => setCopiedVariable(null), 2000)
  }

  // Extract variables for the currently selected event type
  const selectedEventType = eventTypes.find((et) => et.notificationEventTypeId === formData.notificationEventTypeId)
  let variables = []
  if (selectedEventType && selectedEventType.availableVariables) {
    try {
      const parsedVars = JSON.parse(selectedEventType.availableVariables)
      variables = parsedVars.map((v) => v.FieldName || v.fieldName || v.name || v)
    } catch (e) {
      console.error('Failed to parse availableVariables', e)
    }
  }

  if (isLoading) {
    return <div className="p-8 text-center">{t('common.loading', 'Loading...')}</div>
  }

  return (
    <>
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={handleBack}>
          <CaretLeftIcon size={18} />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            {isEditing ? t('notifications.editTitle', 'Edit Template') : t('notifications.createTitle', 'Create Template')}
          </h1>
          <p className="text-sm text-muted-foreground">
            {t('notifications.designerDescription', 'Design your notification template using dynamic variables.')}
          </p>
        </div>
        <div className="ml-auto">
          <Button onClick={handleSave} disabled={isSaving}>
            {isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
          </Button>
        </div>
      </div>

      {error ? (
        <p className="mb-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {error}
        </p>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardContent className="pt-6 space-y-4">
              <div className="grid gap-2 text-sm font-medium">
                {t('notifications.fields.name', 'Template Name')}
                <Input
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  placeholder={t('notifications.placeholders.name', 'e.g. Welcome Email')}
                />
              </div>

              <div className="grid gap-4">
                <div className="grid grid-cols-3 gap-4">
                  <div className="grid gap-2 text-sm font-medium">
                    {t('notifications.fields.channel', 'Channel')}
                    <select
                      name="channel"
                      value={formData.channel}
                      onChange={handleChange}
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      <option value="EMAIL">Email</option>
                      <option value="SMS">SMS</option>
                      <option value="PUSH">Push Notification</option>
                      <option value="IN_APP">In-App</option>
                    </select>
                  </div>
                  <div className="grid gap-2 text-sm font-medium">
                    {t('notifications.fields.language', 'Language')}
                    <select
                      name="language"
                      value={formData.language}
                      onChange={handleChange}
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      <option value="vi">Vietnamese (vi)</option>
                      <option value="en">English (en)</option>
                    </select>
                  </div>
                  <div className="grid gap-2 text-sm font-medium">
                    {t('notifications.fields.status', 'Trạng thái')}
                    <select
                      name="isActive"
                      value={formData.isActive ? 'true' : 'false'}
                      onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.value === 'true' }))}
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      <option value="true">{t('common.active', 'Kích hoạt')}</option>
                      <option value="false">{t('common.inactive', 'Ngừng hoạt động')}</option>
                    </select>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6 space-y-4">
              <div className="grid gap-2 text-sm font-medium">
                {t('notifications.fields.titleTemplate', 'Title Template')}
                <HighlightedInput
                  name="titleTemplate"
                  value={formData.titleTemplate}
                  onChange={handleChange}
                  onSelect={handleSelect}
                  onClick={handleSelect}
                  onKeyUp={handleSelect}
                  placeholder={t('notifications.placeholders.titleTemplate', 'e.g. Welcome {{CustomerName}}!')}
                />
              </div>

              <div className="grid gap-2 text-sm font-medium">
                {t('notifications.fields.bodyTemplate', 'Body Template')}
                <HighlightedTextarea
                  name="bodyTemplate"
                  value={formData.bodyTemplate}
                  onChange={handleChange}
                  onSelect={handleSelect}
                  onClick={handleSelect}
                  onKeyUp={handleSelect}
                  rows={10}
                  placeholder={t('notifications.placeholders.bodyTemplate', 'Enter your message content here...')}
                />
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card className="sticky top-6">
            <div className="border-b border-border px-6 py-4">
              <h3 className="font-semibold text-foreground">
                {t('notifications.variables.title', 'Available Variables')}
              </h3>
              <p className="mt-1 text-xs text-muted-foreground">
                {t('notifications.variables.description', 'Click to copy variables and paste them into your templates.')}
              </p>
            </div>
            <CardContent className="p-0">
              {!formData.notificationEventTypeId ? (
                <div className="p-6 text-center text-sm text-muted-foreground">
                  {t('notifications.variables.selectEventPrompt', 'Select an Event Type first to see available variables.')}
                </div>
              ) : variables.length === 0 ? (
                <div className="p-6 text-center text-sm text-muted-foreground">
                  {t('notifications.variables.none', 'No variables available for this event type.')}
                </div>
              ) : (
                <ul className="divide-y divide-border">
                  {variables.map((v, idx) => (
                    <li 
                      key={idx} 
                      className="flex items-center justify-between px-6 py-3 transition-colors hover:bg-muted/30 cursor-pointer"
                      onClick={() => insertVariable(v)}
                    >
                      <code className="rounded bg-muted px-2 py-1 text-xs font-semibold text-primary">
                        {`{{${v}}}`}
                      </code>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-7 w-7"
                        onClick={(e) => { e.stopPropagation(); insertVariable(v); }}
                        title={t('common.insert', 'Insert')}
                      >
                        {copiedVariable === v ? (
                          <CheckIcon size={14} className="text-success" />
                        ) : (
                          <CopyIcon size={14} className="text-muted-foreground" />
                        )}
                      </Button>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </>
  )
}
