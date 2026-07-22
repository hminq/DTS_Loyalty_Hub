import { CaretLeftIcon, CopyIcon, CheckIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'

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

export function NotificationTemplateDesignerPage() {
  const { id } = useParams()
  const isEditing = id !== 'new' && Boolean(id)
  const { t } = useTranslation()
  const navigate = useNavigate()

  const [formData, setFormData] = useState({
    name: '',
    eventTypeCode: '',
    channel: 'EMAIL',
    language: 'vi',
    titleTemplate: '',
    bodyTemplate: '',
  })
  
  const [eventTypes, setEventTypes] = useState([])
  const [isLoading, setIsLoading] = useState(isEditing)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const [copiedVariable, setCopiedVariable] = useState(null)

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
            eventTypeCode: template.eventTypeCode || '',
            channel: template.channel || 'EMAIL',
            language: template.language || 'vi',
            titleTemplate: template.titleTemplate || '',
            bodyTemplate: template.bodyTemplate || '',
          })
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
      navigate('/notification-templates')
    } catch (err) {
      setError(err.message || t('errors.saveTemplate', 'Failed to save template.'))
      setIsSaving(false)
    }
  }

  const copyVariable = (varName) => {
    const textToCopy = `{{${varName}}}`
    navigator.clipboard.writeText(textToCopy)
    setCopiedVariable(varName)
    setTimeout(() => setCopiedVariable(null), 2000)
  }

  // Extract variables for the currently selected event type
  const selectedEventType = eventTypes.find((et) => et.eventTypeCode === formData.eventTypeCode)
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
        <Button variant="ghost" size="icon" onClick={() => navigate('/notification-templates')}>
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

              <div className="grid grid-cols-2 gap-4">
                <div className="grid gap-2 text-sm font-medium">
                  {t('notifications.fields.eventType', 'Event Type')}
                  <select
                    name="eventTypeCode"
                    value={formData.eventTypeCode}
                    onChange={handleChange}
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    <option value="" disabled>{t('notifications.placeholders.selectEventType', 'Select Event Type')}</option>
                    {eventTypes.map((type) => (
                      <option key={type.eventTypeCode} value={type.eventTypeCode}>
                        {type.displayName || type.eventTypeCode}
                      </option>
                    ))}
                  </select>
                </div>
                
                <div className="grid grid-cols-2 gap-4">
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
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6 space-y-4">
              <div className="grid gap-2 text-sm font-medium">
                {t('notifications.fields.titleTemplate', 'Title Template')}
                <Input
                  name="titleTemplate"
                  value={formData.titleTemplate}
                  onChange={handleChange}
                  placeholder={t('notifications.placeholders.titleTemplate', 'e.g. Welcome {{CustomerName}}!')}
                />
              </div>

              <div className="grid gap-2 text-sm font-medium">
                {t('notifications.fields.bodyTemplate', 'Body Template')}
                <textarea
                  name="bodyTemplate"
                  value={formData.bodyTemplate}
                  onChange={handleChange}
                  rows={10}
                  className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
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
              {!formData.eventTypeCode ? (
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
                    <li key={idx} className="flex items-center justify-between px-6 py-3 transition-colors hover:bg-muted/30">
                      <code className="rounded bg-muted px-2 py-1 text-xs font-semibold text-primary">
                        {`{{${v}}}`}
                      </code>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-7 w-7"
                        onClick={() => copyVariable(v)}
                        title={t('common.copy', 'Copy')}
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
