import { useTranslation } from 'react-i18next'

import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatDateTime, getStatusBadgeVariant } from './eventDefinitionFormatters'

export function EventDefinitionDetails({ detail = {} }) {
  const { t } = useTranslation()

  const statusVariant = getStatusBadgeVariant(detail.status)
  const statusLabel = t(`eventDefinitions.statuses.${detail.status}`, detail.status)

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle>{t('eventDefinitions.detail.metadataTitle')}</CardTitle>
        <Badge variant={statusVariant}>{statusLabel}</Badge>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="text-xs font-medium text-muted-foreground">
              {t('eventDefinitions.form.code')}
            </label>
            <div className="font-mono text-sm font-semibold text-foreground">
              {detail.code || '—'}
            </div>
          </div>

          <div>
            <label className="text-xs font-medium text-muted-foreground">
              {t('eventDefinitions.form.routingKey')}
            </label>
            <div className="font-mono text-sm font-semibold text-foreground">
              {detail.routingKey || '—'}
            </div>
          </div>

          <div>
            <label className="text-xs font-medium text-muted-foreground">
              {t('eventDefinitions.form.name')}
            </label>
            <div className="text-sm font-medium text-foreground">
              {detail.name || '—'}
            </div>
          </div>

          <div>
            <label className="text-xs font-medium text-muted-foreground">
              {t('eventDefinitions.detail.createdAt')}
            </label>
            <div className="text-sm text-muted-foreground">
              {formatDateTime(detail.createdAt)}
            </div>
          </div>
        </div>

        {detail.description && (
          <div>
            <label className="text-xs font-medium text-muted-foreground">
              {t('eventDefinitions.form.description')}
            </label>
            <div className="text-sm text-foreground whitespace-pre-wrap">
              {detail.description}
            </div>
          </div>
        )}

        <div className="flex flex-wrap gap-2 pt-2 border-t border-border text-xs text-muted-foreground">
          <span>
            {t('eventDefinitions.detail.canEditIdentity')}:{' '}
            <strong className="text-foreground">{detail.canEditIdentity ? t('common.yes', 'Yes') : t('common.no', 'No')}</strong>
          </span>
          <span>•</span>
          <span>
            {t('eventDefinitions.detail.canCreateDraft')}:{' '}
            <strong className="text-foreground">{detail.canCreateDraft ? t('common.yes', 'Yes') : t('common.no', 'No')}</strong>
          </span>
          <span>•</span>
          <span>
            {t('eventDefinitions.detail.canRetire')}:{' '}
            <strong className="text-foreground">{detail.canRetire ? t('common.yes', 'Yes') : t('common.no', 'No')}</strong>
          </span>
        </div>
      </CardContent>
    </Card>
  )
}
