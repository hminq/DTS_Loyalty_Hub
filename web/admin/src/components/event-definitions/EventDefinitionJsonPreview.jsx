import { useTranslation } from 'react-i18next'

import { buildCanonicalPayloadSchema } from './eventDefinitionPayloads'

export function EventDefinitionJsonPreview({ fields = [], targets = [] }) {
  const { t } = useTranslation()
  const canonicalSchema = buildCanonicalPayloadSchema(fields, targets)
  const jsonString = JSON.stringify(canonicalSchema, null, 2)

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
          {t('eventDefinitions.schema.jsonPreviewTitle')}
        </h4>
        <span className="text-[11px] text-muted-foreground">
          {t('eventDefinitions.schema.readOnlyPreview')}
        </span>
      </div>
      <pre className="max-h-80 overflow-auto rounded-md border border-border bg-muted/40 p-3 font-mono text-xs text-foreground">
        {jsonString}
      </pre>
    </div>
  )
}
