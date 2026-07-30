import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'

function NotificationTemplateDetails({ template, language, t }) {
  if (!template) return null

  const variables = template.variables || []

  return (
    <div className="mt-5 grid gap-5 lg:grid-cols-3">
      {/* Main Content (Left / Middle 2 cols) */}
      <div className="space-y-5 lg:col-span-2">
        {/* General Information Card */}
        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 p-4 pb-3">
            <CardTitle className="text-sm font-semibold">
              {t('notifications.detail.generalTitle', 'General information')}
            </CardTitle>
            <Badge variant={template.isActive ? 'success' : 'secondary'}>
              {template.isActive
                ? t('notifications.status.active', 'Active')
                : t('notifications.status.inactive', 'Inactive')}
            </Badge>
          </CardHeader>
          <CardContent className="grid gap-5 p-4 text-[13px]">
            <div className="grid gap-4 sm:grid-cols-2">
              <DetailItem
                label={t('notifications.fields.name', 'Template name')}
                value={template.name}
                prominent
              />
              <DetailItem
                label={t('notifications.fields.notificationCode', 'Notification code')}
                value={
                  <span className="inline-flex items-center rounded-md border border-border/80 bg-muted/50 px-2.5 py-1 font-mono text-[13px] font-bold text-foreground">
                    {template.notificationCode || '—'}
                  </span>
                }
              />
            </div>

            <div className="grid gap-4 border-t border-border/50 pt-4 sm:grid-cols-3">
              <DetailItem
                label={t('notifications.fields.channel', 'Channel')}
                value={<Badge variant="outline">{template.channel || '—'}</Badge>}
              />
              <DetailItem
                label={t('notifications.fields.language', 'Language')}
                value={template.language === 'vi' ? 'Vietnamese (vi)' : 'English (en)'}
              />
              <DetailItem
                label={t('notifications.fields.status', 'Status')}
                value={
                  <Badge variant={template.isActive ? 'success' : 'secondary'}>
                    {template.isActive
                      ? t('notifications.status.active', 'Active')
                      : t('notifications.status.inactive', 'Inactive')}
                  </Badge>
                }
              />
            </div>

            <div className="grid gap-4 border-t border-border/50 pt-4 sm:grid-cols-3">
              <DetailItem
                label={t('notifications.detail.templateId', 'Template ID')}
                value={template.templateId}
                mono
              />
              <DetailItem
                label={t('common.createdAt', 'Created at')}
                value={formatDateTime(template.createdAt, language)}
                muted
              />
              <DetailItem
                label={t('notifications.columns.updatedAt', 'Updated at')}
                value={formatDateTime(template.updatedAt, language)}
                muted
              />
            </div>
          </CardContent>
        </Card>

        {/* Template Content Preview Card */}
        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="p-4 pb-3">
            <CardTitle className="text-sm font-semibold">
              {t('notifications.detail.contentTitle', 'Template content')}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 p-4 text-[13px]">
            <div>
              <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
                {t('notifications.fields.titleTemplate', 'Title template')}
              </p>
              <div className="mt-1.5 rounded-lg border border-border/70 bg-muted/20 p-3 font-medium text-foreground leading-relaxed">
                <HighlightedTemplateText text={template.titleTemplate} />
              </div>
            </div>

            <div>
              <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
                {t('notifications.fields.bodyTemplate', 'Body template')}
              </p>
              <div className="mt-1.5 rounded-lg border border-border/70 bg-muted/20 p-4 font-medium text-foreground leading-relaxed whitespace-pre-wrap">
                <HighlightedTemplateText text={template.bodyTemplate} />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Configured Variables Card (Right 1 col) */}
      <div className="space-y-5 lg:col-span-1">
        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="p-4 pb-3">
            <CardTitle className="text-sm font-semibold">
              {t('notifications.variables.title', 'Available Variables')}
            </CardTitle>
            <p className="mt-0.5 text-xs text-muted-foreground">
              {t('notifications.variables.description', 'Variables saved in this template configuration.')}
            </p>
          </CardHeader>
          <CardContent className="p-0">
            {variables.length === 0 ? (
              <div className="p-6 text-center text-xs text-muted-foreground">
                {t('notifications.variables.none', 'No variables configured for this template.')}
              </div>
            ) : (
              <ul className="divide-y divide-border/60">
                {variables.map((variable, index) => (
                  <li key={`${variable.name}-${index}`} className="px-4 py-3 text-xs">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <span className="font-mono text-sm font-bold text-primary">
                        {`{{${variable.name}}}`}
                      </span>
                      <Badge variant="outline" className="text-[11px]">
                        {variable.sourceType || 'INPUT'}
                      </Badge>
                    </div>
                    {variable.description ? (
                      <p className="mt-1 text-muted-foreground break-words">{variable.description}</p>
                    ) : null}
                    {variable.fixedValue ? (
                      <p className="mt-1 text-muted-foreground">
                        <span className="font-medium text-foreground">Fixed:</span> {variable.fixedValue}
                      </p>
                    ) : null}
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

function HighlightedTemplateText({ text }) {
  if (!text) return <span className="text-muted-foreground">—</span>

  const parts = text.split(/(\{\{\s*[A-Za-z0-9_]+\s*\}\})/g)

  return (
    <span>
      {parts.map((part, index) => {
        if (/^\{\{\s*[A-Za-z0-9_]+\s*\}\}$/.test(part)) {
          return (
            <span
              key={index}
              className="mx-0.5 inline-flex items-center rounded border border-primary/30 bg-primary/10 px-1.5 py-0.5 font-mono text-xs font-semibold text-primary"
            >
              {part}
            </span>
          )
        }
        return part
      })}
    </span>
  )
}

function DetailItem({ label, value, mono = false, muted = false, prominent = false }) {
  const valueClass = [
    'mt-1 break-words',
    prominent ? 'text-base font-semibold text-foreground' : 'text-sm font-medium',
    mono ? 'font-mono text-xs' : '',
    muted ? 'text-muted-foreground' : 'text-foreground',
  ].filter(Boolean).join(' ')

  return (
    <div className="min-w-0">
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
        {label}
      </p>
      {typeof value === 'string' || typeof value === 'number' ? (
        <p className={valueClass}>{value || '—'}</p>
      ) : (
        <div className={valueClass}>{value || '—'}</div>
      )}
    </div>
  )
}

function formatDateTime(value, language) {
  if (!value) return '—'
  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export { NotificationTemplateDetails }
