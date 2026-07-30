import { CopyIcon, EyeIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatDateTime, getStatusBadgeVariant } from './eventDefinitionFormatters'

export function EventDefinitionVersionHistory({
  eventTypeId,
  versions = [],
  canCreateDraft = false,
  hasUpdatePermission = false,
  onCloneVersion,
}) {
  const { t } = useTranslation()

  return (
    <Card className="overflow-hidden shadow-none">
      <CardHeader className="border-b border-border">
        <CardTitle>{t('eventDefinitions.detail.versionHistoryTitle')}</CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        {versions.length === 0 ? (
          <div className="p-5 text-sm text-muted-foreground">
            {t('eventDefinitions.detail.noVersions')}
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] border-collapse text-left text-[13px]">
              <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.version')}</th>
                  <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.status')}</th>
                  <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.createdAt')}</th>
                  <th className="px-4 py-2.5 font-semibold">{t('eventDefinitions.columns.publishedAt')}</th>
                  <th className="px-4 py-2.5 text-right font-semibold">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {versions.map((ver) => {
                  const statusVariant = getStatusBadgeVariant(ver.status)
                  const statusLabel = t(`eventDefinitions.versionStatuses.${ver.status}`, ver.status)

                  const isPublished = ver.status === 'PUBLISHED'
                  const canClone = isPublished && canCreateDraft && hasUpdatePermission

                  return (
                    <tr
                      key={ver.eventTypeVersionId}
                      className="border-t border-border transition-colors hover:bg-muted/25"
                    >
                      <td className="px-4 py-3 font-semibold text-foreground">
                        <Link
                          to={`/event-definitions/${eventTypeId}/versions/${ver.eventTypeVersionId}`}
                          className="hover:text-primary hover:underline"
                        >
                          v{ver.version}
                        </Link>
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant={statusVariant}>{statusLabel}</Badge>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatDateTime(ver.createdAt)}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatDateTime(ver.publishedAt)}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Link to={`/event-definitions/${eventTypeId}/versions/${ver.eventTypeVersionId}`}>
                            <Button variant="ghost" size="sm" className="h-8 px-2">
                              <EyeIcon data-icon="inline-start" />
                              {t('common.view')}
                            </Button>
                          </Link>

                          {canClone && (
                            <Button
                              variant="outline"
                              size="sm"
                              className="h-8 px-2"
                              onClick={() => onCloneVersion?.(ver)}
                            >
                              <CopyIcon data-icon="inline-start" />
                              {t('eventDefinitions.actions.clone')}
                            </Button>
                          )}
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
